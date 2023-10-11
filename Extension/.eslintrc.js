/**@type {import('eslint').Linter.Config} */
// eslint-disable-next-line no-undef
export default {
	root: true,
	parser: '@typescript-eslint/parser',
	plugins: [
		'@typescript-eslint'
	],
	extends: [
		'eslint:recommended',
		'plugin:@typescript-eslint/recommended'
	],
	rules: {
		'@typescript-eslint/explicit-module-boundary-types': 0,
		'@typescript-eslint/no-explicit-any': 0,
		'@typescript-eslint/no-non-null-assertion': 0,
		'@typescript-eslint/no-unused-vars': 0,
		'semi': [2, "always"]
	}
};