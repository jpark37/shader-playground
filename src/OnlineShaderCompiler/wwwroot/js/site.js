$(function () {
    /**
     * @typedef {('TextBox'|'ComboBox'|'CheckBox')} ShaderProcessorParameterType
     */

    /**
     * @typedef ShaderProcessorParameter
     * @property {string} Name
     * @property {string} DisplayName
     * @property {ShaderProcessorParameterType} ParameterType
     * @property {string[]} Options
     * @property {string} DefaultValue
     */

    /**
     * @typedef ShaderProcessor
     * @property {string} Name
     * @property {string} DisplayName
     * @property {ShaderProcessorParameter[]} Parameters
     */

    /**
     * @typedef ShaderLanguage
     * @property {string} Name
     * @property {string} DefaultCode
     * @property {ShaderProcessorParameter[]} LanguageParameters
     * @property {ShaderProcessor[]} Processors
     */

    /**
     * @param {ShaderProcessorParameter} parameter
     */
    function createParameterEditor(parameter) {
        switch (parameter.ParameterType) {
            case "TextBox":
                var textBox = document.createElement("input");
                textBox.type = "text";
                textBox.setAttribute("class", "form-control form-control-sm");
                textBox.defaultValue = parameter.DefaultValue;
                return textBox;

            case "ComboBox":
                var comboBox = document.createElement("select");
                comboBox.setAttribute("class", "form-control form-control-sm");
                for (var option of parameter.Options) {
                    var isSelected = (option === parameter.DefaultValue);
                    comboBox.options.add(new Option(option, option, isSelected, isSelected));
                }
                return comboBox;

            case "CheckBox":
                var checkBox = document.createElement("input");
                checkBox.setAttribute("class", "form-check-input");
                checkBox.type = "checkbox";
                checkBox.checked = (parameter.DefaultValue === "true");
                return checkBox;

            default:
                throw `Unexpected parameter type: ${parameter.ParameterType}`;
        }
    }

    /**
     * @param {ShaderProcessorParameter} parameter
     */
    function createParameterGroup(parameter, onChanged) {
        var parameterGroup = document.createElement("div");
        var formGroupClass = "form-group";
        if (parameter.ParameterType === "CheckBox") {
            formGroupClass += " form-check";
        }
        parameterGroup.setAttribute("class", formGroupClass);

        var parameterLabel = document.createElement("label");
        parameterLabel.setAttribute("class", "text-secondary");
        parameterLabel.setAttribute("for", parameter.Name);
        parameterLabel.textContent = parameter.DisplayName;
        if (parameter.ParameterType !== "CheckBox") {
            parameterGroup.appendChild(parameterLabel);
        }

        var parameterEditor = createParameterEditor(parameter);
        parameterEditor.addEventListener('change', onChanged);
        parameterEditor.dataset['submitargument'] = parameter.Name;
        parameterGroup.appendChild(parameterEditor);

        if (parameter.ParameterType === "CheckBox") {
            parameterLabel.setAttribute("class", "text-secondary form-check-label");
            parameterGroup.appendChild(parameterLabel);
        }

        return parameterGroup;
    }

    /**
     * @param {HTMLDivElement} argumentsDiv
     * @param {ShaderProcessorParameter[]} parameters
     */
    function updateArguments(argumentsDiv, parameters, onChanged) {
        argumentsDiv.innerHTML = '';

        for (var parameter of parameters) {
            argumentsDiv.appendChild(createParameterGroup(parameter, onChanged));
        }
    }

    /** @type {ShaderLanguage[]} */
    var shaderLanguages = window.ShaderLanguages;

    /** @type {HTMLTextAreaElement} */
    var codeTextArea = document.getElementById('Code');

    var codeEditor = CodeMirror.fromTextArea(codeTextArea, {
        mode: "x-shader/hlsl",
        theme: "neat",
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4
    });

    /** @type {HTMLSelectElement} */
    var languageSelect = document.getElementById('language');
    for (var language of window.ShaderLanguages) {
        languageSelect.options.add(new Option(language.Name, language.Name));
    }

    /** @type {HTMLSelectElement} */
    var processorSelect = document.getElementById("processor");

    var languageArgumentsDiv = document.getElementById('language-arguments');
    var processorArgumentsDiv = document.getElementById('processor-arguments');

    var outputTabsDiv = document.getElementById('output-tabs');
    var outputContainerDiv = document.getElementById('output-container');

    function getSelectedLanguage() {
        return shaderLanguages.find(x => x.Name === languageSelect.selectedOptions[0].value);
    }

    function getSelectedProcessor() {
        var language = getSelectedLanguage();
        return language.Processors.find(x => x.Name === processorSelect.selectedOptions[0].value);
    }

    /**
     * @param {HTMLElement} element
     */
    function getValue(element) {
        switch (element.tagName) {
            case "SELECT":
                return element.selectedOptions[0].value;

            case "TEXTAREA":
                return element.value;

            case "INPUT":
                switch (element.type) {
                    case "text":
                        return element.value;

                    case "checkbox":
                        return element.checked ? "true" : "false";

                    default:
                        throw `Unexpected input element type: ${element.type}`;
                }

            default:
                throw `Unexpected element type: ${element.tagName}`;
        }
    }

    function getCodeMirrorMode(language) {
        switch (language) {
            case "DXBC":
                return "text/x-dxbc";

            case "DXIL":
                return "text/x-llvm-ir";

            default:
                return "text/plain";
        }
    }

    function compileCode() {
        /** @type {HTMLElement[]} */
        var submitElements = document.querySelectorAll("[data-submit]");

        codeTextArea.value = codeEditor.getValue();

        var jsonObject = {};
        for (var element of submitElements) {
            jsonObject[element.dataset.submit] = getValue(element);
        }

        /** @type {HTMLElement[]} */
        var submitArgumentElements = document.querySelectorAll("[data-submitargument]");
        var jsonArguments = {};
        for (var element of submitArgumentElements) {
            jsonArguments[element.dataset.submitargument] = getValue(element);
        }
        jsonObject.arguments = jsonArguments;

        $.ajax({
            url: window.CompileUrl,
            type: "POST",
            data: JSON.stringify(jsonObject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                //finishLoading();
                alert("Unexpected error");
            },
            success: function (response) {
                //finishLoading();
                // TODO

                outputTabsDiv.innerHTML = '';
                
                for (var output of response.outputs) {
                    var outputTabButton = document.createElement('button');
                    outputTabButton.setAttribute('class', 'btn btn-outline-secondary');
                    outputTabButton.textContent = output.displayName;

                    (function () {
                        var localOutput = output;
                        outputTabButton.onclick = () => {
                            outputContainerDiv.innerHTML = '';

                            CodeMirror(outputContainerDiv, {
                                value: localOutput.value,
                                mode: getCodeMirrorMode(localOutput.language),
                                theme: "neat",
                                lineNumbers: true,
                                matchBrackets: true,
                                readOnly: true
                            });
                        };
                    })();

                    outputTabsDiv.appendChild(outputTabButton);
                }

                // TODO: Keep existing tab selected.
                outputTabsDiv.children[0].click();
            }
        });
    }

    var compileCodeTimeout;

    function somethingChanged() {
        clearTimeout(compileCodeTimeout);
        compileCodeTimeout = setTimeout(compileCode, 500);
    }

    codeEditor.on('change', somethingChanged);

    /**
     * @param {HTMLSelectElement} element
     */
    function raiseSelectElementChange(element) {
        var event = document.createEvent('Event');
        event.initEvent('change');
        element.dispatchEvent(event);
    }

    languageSelect.addEventListener(
        "change",
        () => {
            var language = getSelectedLanguage();

            codeEditor.setValue(language.DefaultCode);

            updateArguments(languageArgumentsDiv, language.LanguageParameters, somethingChanged);

            while (processorSelect.options.length > 0) {
                processorSelect.options.remove(0);
            }
            for (var processor of language.Processors) {
                processorSelect.options.add(new Option(processor.DisplayName, processor.Name));
            }

            raiseSelectElementChange(processorSelect);

            somethingChanged();
        });

    processorSelect.addEventListener(
        "change",
        () => {
            var processor = getSelectedProcessor();
            updateArguments(processorArgumentsDiv, processor.Parameters, somethingChanged);

            somethingChanged();
        });

    raiseSelectElementChange(languageSelect);
});