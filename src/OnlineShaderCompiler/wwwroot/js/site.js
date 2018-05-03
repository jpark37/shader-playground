$(function () {
    var codeEditor = CodeMirror(document.getElementById('code-editor-container'), {
        value: $("#Code").val(),
        mode: "x-shader/hlsl",
        theme: "neat",
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4
    });

    var codeEditor = CodeMirror(document.getElementById('compiler-output-container'), {
        mode: "x-shader/llvm-ir",
        theme: "neat",
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4,
        readOnly: true
    });
});