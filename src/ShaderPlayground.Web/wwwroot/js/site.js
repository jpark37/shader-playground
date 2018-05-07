$(function () {
    /**
     * @typedef {('TextBox'|'ComboBox'|'CheckBox')} ShaderCompilerParameterType
     */

    /**
     * @typedef ShaderCompilerParameter
     * @property {string} name
     * @property {string} displayName
     * @property {ShaderCompilerParameterType} parameterType
     * @property {string[]} options
     * @property {string} defaultValue
     */

    /**
     * @typedef ShaderCompiler
     * @property {string} name
     * @property {string} displayName
     * @property {ShaderCompilerParameter[]} parameters
     */

    /**
     * @typedef ShaderLanguage
     * @property {string} name
     * @property {string} defaultCode
     * @property {ShaderCompilerParameter[]} languageParameters
     * @property {ShaderCompiler[]} compilers
     */

    /**
     * @param {ShaderCompilerParameter} parameter
     */
    function createParameterEditor(parameter) {
        switch (parameter.parameterType) {
            case "TextBox":
                var textBox = document.createElement("input");
                textBox.type = "text";
                textBox.setAttribute("class", "form-control form-control-sm");
                textBox.defaultValue = parameter.defaultValue;
                return textBox;

            case "ComboBox":
                var comboBox = document.createElement("select");
                comboBox.setAttribute("class", "form-control form-control-sm");
                for (var option of parameter.options) {
                    var isSelected = (option === parameter.defaultValue);
                    comboBox.options.add(new Option(option, option, isSelected, isSelected));
                }
                return comboBox;

            case "CheckBox":
                var checkBox = document.createElement("input");
                checkBox.setAttribute("class", "form-check-input");
                checkBox.type = "checkbox";
                checkBox.checked = (parameter.defaultValue === "true");
                return checkBox;

            default:
                throw `Unexpected parameter type: ${parameter.parameterType}`;
        }
    }

    /**
     * @param {ShaderCompilerParameter} parameter
     * @param {boolean} addLabel
     */
    function createParameterGroup(parameter, addLabel, onChanged) {
        var parameterGroup = document.createElement("div");
        var formGroupClass = "form-group";
        if (parameter.parameterType === "CheckBox") {
            formGroupClass += " form-check";
        }
        parameterGroup.setAttribute("class", formGroupClass);

        var parameterID = `Parameter${parameter.name}`;

        if (addLabel) {
            var parameterLabel = document.createElement("label");
            parameterLabel.setAttribute("class", "text-secondary");
            parameterLabel.setAttribute("for", parameterID);
            parameterLabel.textContent = parameter.displayName;
            if (parameter.ParameterType !== "CheckBox") {
                parameterGroup.appendChild(parameterLabel);
            }
        }

        var parameterEditor = createParameterEditor(parameter);
        parameterEditor.id = parameterID;
        parameterEditor.addEventListener('change', onChanged);
        parameterEditor.dataset['submitargument'] = parameter.name;
        parameterEditor.title = parameter.displayName;
        if (!addLabel && parameter.parameterType !== "CheckBox") {
            parameterEditor.setAttribute('style', 'width:100px');
        }
        parameterGroup.appendChild(parameterEditor);

        if (addLabel && parameter.parameterType === "CheckBox") {
            parameterLabel.setAttribute("class", "text-secondary form-check-label");
            parameterGroup.appendChild(parameterLabel);
        }

        return parameterGroup;
    }

    /**
     * @param {HTMLDivElement} argumentsDiv
     * @param {ShaderCompilerParameter[]} parameters
     */
    function updateArguments(argumentsDiv, parameters, addLabel, onChanged) {
        argumentsDiv.innerHTML = '';

        for (var parameter of parameters) {
            argumentsDiv.appendChild(createParameterGroup(parameter, addLabel, onChanged));
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
        languageSelect.options.add(new Option(language.name, language.name));
    }

    /** @type {HTMLSelectElement} */
    var compilerSelect = document.getElementById("compiler");

    var languageArgumentsDiv = document.getElementById('language-arguments');
    var compilerArgumentsDiv = document.getElementById('compiler-arguments');

    var outputTabsDiv = document.getElementById('output-tabs');
    var outputContainerDiv = document.getElementById('output-container');

    function getSelectedLanguage() {
        return shaderLanguages.find(x => x.name === languageSelect.selectedOptions[0].value);
    }

    function getSelectedCompiler() {
        var language = getSelectedLanguage();
        return language.compilers.find(x => x.name === compilerSelect.selectedOptions[0].value);
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

        $("#output-container").addClass("loading");
        $("#output-loading").show();

        function finishLoading() {
            $("#output-loading").hide();
            $("#output-container").removeClass("loading");
        }

        $.ajax({
            url: window.CompileUrl,
            type: "POST",
            data: JSON.stringify(jsonObject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                finishLoading();
                alert("Unexpected error");
            },
            success: function (response) {
                finishLoading();

                var selectedOutputTab = document.querySelector('input[name="output-tab-selector"]:checked');
                var selectedOutputTabIndex;
                if (response.selectedOutputIndex !== null) {
                    selectedOutputTabIndex = response.selectedOutputIndex;
                } else if (selectedOutputTab !== null) {
                    selectedOutputTabIndex = response.outputs.findIndex(x => x.displayName === selectedOutputTab.value);
                    if (selectedOutputTabIndex === -1) {
                        selectedOutputTabIndex = 0;
                    }
                } else {
                    selectedOutputTabIndex = 0;
                }

                outputTabsDiv.innerHTML = '';
                
                for (var i = 0; i < response.outputs.length; i++) {
                    var output = response.outputs[i];

                    var outputTabButton = document.createElement('label');
                    outputTabButton.setAttribute('class', 'btn btn-outline-secondary');

                    var radioButton = document.createElement('input');
                    radioButton.type = 'radio';
                    radioButton.name = 'output-tab-selector';
                    radioButton.value = output.displayName;
                    outputTabButton.appendChild(radioButton);

                    var radioButtonText = document.createTextNode(output.displayName);
                    outputTabButton.appendChild(radioButtonText);

                    (function () {
                        var localOutput = output;

                        function selectTab() {
                            outputContainerDiv.innerHTML = '';

                            CodeMirror(outputContainerDiv, {
                                value: localOutput.value,
                                mode: getCodeMirrorMode(localOutput.language),
                                theme: "neat",
                                lineNumbers: true,
                                matchBrackets: true,
                                readOnly: true
                            });
                        }

                        outputTabButton.onclick = selectTab;

                        if (i === selectedOutputTabIndex) {
                            selectTab();
                            $(outputTabButton).addClass('active');
                            radioButton.checked = true;
                        }
                    })();

                    outputTabsDiv.appendChild(outputTabButton);
                }
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

            codeEditor.setValue(language.defaultCode);

            updateArguments(languageArgumentsDiv, language.languageParameters, false, somethingChanged);

            while (compilerSelect.options.length > 0) {
                compilerSelect.options.remove(0);
            }
            for (var compiler of language.compilers) {
                compilerSelect.options.add(new Option(compiler.displayName, compiler.name));
            }

            raiseSelectElementChange(compilerSelect);

            somethingChanged();
        });

    compilerSelect.addEventListener(
        "change",
        () => {
            var compiler = getSelectedCompiler();
            updateArguments(compilerArgumentsDiv, compiler.parameters, true, somethingChanged);
            document.getElementById('compiler-description').textContent = compiler.description;

            somethingChanged();
        });

    raiseSelectElementChange(languageSelect);
});