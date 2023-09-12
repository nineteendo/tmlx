"""Save color palettes."""
from json import load
from os.path import join
from re import match
from struct import pack
from typing import Literal, TypedDict, TypeGuard, get_args
from zlib import compress, crc32

# Linting arguments
# pylint: disable=too-many-arguments

PNG_SIGNATURE: bytes = b'\x89PNG\r\n\x1a\n'

# Level 1 type hints
Channel = int
ColorType = Literal[0, 2, 3, 4, 6]
CompressionMethod = Literal[0]
FilterMethod = Literal[0]
HexColor = str
PaletteMapping = dict[str, str]
InterlaceMethod = Literal[0, 1]

# Level 2 type hints
ChannelList = list[Channel]
Channels = tuple[Channel, ...]
ColorEncoding = tuple[int, ColorType]
ImageRow = list[HexColor]


class PalettePack(TypedDict):
    """Pack of palettes."""

    packName: str  # noqa: N815, screw the back end
    packId: str  # noqa: N815, screw the back end
    link: str
    paletteMapping: PaletteMapping  # noqa: N815, screw the back end


Rgba = tuple[Channel, Channel, Channel, Channel]

# Level 3 type hints
Image = list[ImageRow]
PalettePacks = list[PalettePack]


# Level 4 type hints
class PalettePacksObject(TypedDict):
    """Palette packs object."""

    palettePacks: PalettePacks  # noqa: N815, screw the back end


Palettes = dict[str, Image]


def get_chunk(chunk_type: bytes, chunk_data: bytes) -> bytes:
    """Get png chunk."""
    length: bytes = pack('>I', len(chunk_data))
    crc:    bytes = pack('>I', crc32(chunk_type + chunk_data))
    return length + chunk_type + chunk_data + crc


def get_ihdr_chunk(
    width: int, height: int, bit_depth: int, color_type: ColorType,
    compression_method: CompressionMethod, filter_method: FilterMethod,
    interlace_method: InterlaceMethod
) -> bytes:
    """Get png image header chunk."""
    ihdr_chunk_data: bytes = pack(
        '>IIBBBBB', width, height, bit_depth, color_type, compression_method,
        filter_method, interlace_method
    )
    return get_chunk(b'IHDR', ihdr_chunk_data)


def hex2rgba64(color: HexColor) -> Rgba:
    """Convert hex color to rgba64."""
    if not match('#[a-fA-F0-9]*$', color) or len(color) not in (4, 5, 7, 9):
        raise ValueError(
            'Invalid color format. Use #RGB, #RGBA, #RRGGBB, or #RRGGBBAA'
        )

    if len(color) == 4:
        # Convert #RGB to #RRGGBB format
        color = f'#{color[1] * 2}{color[2] * 2}{color[3] * 2}'
    elif len(color) == 5:
        # Convert #RGBA to #RRGGBBAA format
        color = f'#{color[1] * 2}{color[2] * 2}{color[3] * 2}{color[4] * 2}'

    red:   Channel = Channel(color[1:3] * 2, 16)
    green: Channel = Channel(color[3:5] * 2, 16)
    blue:  Channel = Channel(color[5:7] * 2, 16)
    alpha: Channel = Channel(color[7:9] * 2, 16) if len(color) == 9 else 65535
    return red, green, blue, alpha


def has_redundancy(bit_depth: int, *channels: Channel) -> bool:
    """Check if channels have redundancy."""
    bit_mask: int = (1 << bit_depth) - 1
    return all(
        (channel >> bit_depth) & bit_mask == channel & bit_mask
        for channel in channels
    )


def get_color_encoding(color: HexColor) -> ColorEncoding:
    """Get hex color encoding."""
    red:   Channel
    green: Channel
    blue:  Channel
    alpha: Channel
    red, green, blue, alpha = hex2rgba64(color)
    if not alpha:
        # Fully transparent
        return 8, 4

    if alpha != 65535:
        # Transparent
        return (
            8 if has_redundancy(8, red, green, blue) else 16,
            (4 if red == green == blue else 6)
        )

    # Opaque
    if not red == green == blue:
        # Truecolor
        return 8 if has_redundancy(8, red, green, blue) else 16, 2

    # Greyscale
    bit_depth: int
    if not has_redundancy(8, red):
        # 16bit
        bit_depth = 16
    elif not has_redundancy(4, red):
        # 8bit
        bit_depth = 8
    elif not has_redundancy(2, red):
        bit_depth = 4
    else:
        bit_depth = 1 if has_redundancy(1, red) else 2

    return bit_depth, 0


def is_color_type(number: int) -> TypeGuard[ColorType]:
    """Check if number is valid color type."""
    return number in get_args(ColorType)


def get_image_encoding(image: Image) -> ColorEncoding:
    """Get hex color encoding of image."""
    bit_depth:  int = 1
    color_type: ColorType = 0
    for image_row in image:
        for color in image_row:
            new_bit_depth:  int
            new_color_type: ColorType
            new_bit_depth, new_color_type = get_color_encoding(color)
            bit_depth = max(bit_depth, new_bit_depth)
            combined_color_type: int = color_type | new_color_type
            if not is_color_type(combined_color_type):
                raise RuntimeError(
                    f'{combined_color_type} is not a valid color type'
                )

            color_type = combined_color_type

    return bit_depth, color_type


def encode_color(
    color: HexColor, bit_depth: int, color_type: ColorType
) -> Channels:
    """Encode hex color."""
    red:   Channel
    green: Channel
    blue:  Channel
    alpha: Channel
    red, green, blue, alpha = hex2rgba64(color)
    bit_mask: int = (1 << bit_depth) - 1
    red &= bit_mask
    green &= bit_mask
    blue &= bit_mask
    alpha &= bit_mask
    if not color_type:
        return (red,)

    if color_type == 2:
        return red, green, blue

    if color_type == 3:
        raise ValueError('color type 3 is currently not supported')

    if color_type == 4:
        return red, alpha

    return red, green, blue, alpha


def encode_image_row(
    image_row: ImageRow, bit_depth: int, color_type: ColorType
) -> Channels:
    """Encode hex colors of image row."""
    channels: ChannelList = []
    for color in image_row:
        channels.extend(encode_color(color, bit_depth, color_type))

    return tuple(channels)


def pack_channels(channels: Channels, bit_depth: int) -> bytearray:
    """Pack channels in a byte array."""
    packed_channels: bytearray = bytearray()
    current_byte:    int = 0
    bit_count:       int = 0
    for channel in channels:
        current_byte = (current_byte << bit_depth) | channel
        bit_count += bit_depth
        if bit_count >= 8:
            bit_count -= 8
            packed_channels.append(current_byte >> bit_count)
            current_byte &= (1 << bit_count) - 1

    if bit_count:
        current_byte <<= 8 - bit_count
        packed_channels.append(current_byte)

    return packed_channels


def get_idat_chunk(
    image: Image, bit_depth: int, color_type: ColorType
) -> bytes:
    """Get png image data chunk."""
    idat_chunk_data: bytes = b''
    for image_row in image:
        row_data: bytearray = pack_channels(
            encode_image_row(image_row, bit_depth, color_type), bit_depth
        )
        idat_chunk_data += b'\0' + row_data

    return get_chunk(b'IDAT', compress(idat_chunk_data))


def get_png_data(image: Image) -> bytes:
    """Get png data of image."""
    if not image:
        raise ValueError('image is empty')

    width: int = len(image[0])
    if not width:
        raise ValueError('image is empty')

    if not all(len(image_row) == width for image_row in image):
        raise ValueError('image is not rectangular')

    height:     int = len(image)
    bit_depth:  int
    color_type: ColorType
    bit_depth, color_type = get_image_encoding(image)
    compression_method: CompressionMethod = 0
    filter_method:      FilterMethod = 0
    interlace_method:   InterlaceMethod = 0
    ihdr_chunk: bytes = get_ihdr_chunk(
        width, height, bit_depth, color_type, compression_method,
        filter_method, interlace_method
    )
    idat_chunk: bytes = get_idat_chunk(image, bit_depth, color_type)
    iend_chunk: bytes = get_chunk(b'IEND', b'')
    return PNG_SIGNATURE + ihdr_chunk + idat_chunk + iend_chunk


def main() -> None:
    """Save color palette."""
    with open('palettePacks.json', 'rb') as file:
        palette_packs_object: PalettePacksObject = load(file)

    palette_packs: PalettePacks = palette_packs_object['palettePacks']
    for palette_pack in palette_packs:
        pack_id: str = palette_pack['packId']
        palette_mapping: PaletteMapping = palette_pack['paletteMapping']
        with open(join(pack_id, 'palettes.json'), 'rb') as file:
            palettes: Palettes = load(file)

        for palette_id in palette_mapping.values():
            palette: Image = palettes[palette_id]
            with open(join(pack_id, palette_id + '.png'), 'wb') as file:
                png_data: bytes = get_png_data(palette)
                file.write(png_data)


if __name__ == '__main__':
    main()
