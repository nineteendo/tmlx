import {
	createConnection,
	ProposedFeatures,
	CompletionItem,
	CompletionItemKind,
	TextDocumentPositionParams,
} from 'vscode-languageserver/node';

const suggestions = ['black', 'white'].map<CompletionItem>(label => {
	return {
		label,
		kind: CompletionItemKind.Constant,
	};
}).concat(['down', 'left', 'right', 'up', 'write'].map(label => {
	return {
		label,
		kind: CompletionItemKind.Function,
	};
})).concat(['else', 'exit', 'goto', 'if', 'while'].map(label => {
	return {
		label,
		kind: CompletionItemKind.Keyword,
	};
}));

const connection = createConnection(ProposedFeatures.all);

connection.onInitialize((_) => {
	return {
		capabilities: {
			completionProvider: {}
		}
	};
});

connection.onCompletion(
	(_textDocumentPosition: TextDocumentPositionParams): CompletionItem[] => {
		return suggestions;
	}
);

connection.listen();