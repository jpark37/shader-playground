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
     * @property {string} description
     * @property {string[]} inputLanguages
     * @property {ShaderCompilerParameter[]} parameters
     * @property {string[]} outputLanguages
     */

    /**
     * @typedef ShaderLanguage
     * @property {string} name -
     * @property {string} defaultCode -
     */

    var counter = 0;
    function uniqueId() {
        return 'myid-' + counter++;
    }

    class ParameterEditor {
        /**
         * @param {ShaderCompilerParameter} parameter -
         * @returns {ParameterEditor} -
         */
        static create(parameter) {
            switch (parameter.parameterType) {
                case "TextBox":
                    return new TextBoxParameterEditor(parameter);

                case "ComboBox":
                    return new ComboBoxParameterEditor(parameter);

                case "CheckBox":
                    return new CheckBoxParameterEditor(parameter);

                default:
                    throw `Unexpected parameter type: ${parameter.parameterType}`;
            }
        }

        /**
         * @param {ShaderCompilerParameter} parameter
         */
        constructor(parameter, templateId, initializeElementCallback) {
            this.parameter = parameter;

            /** @type {HTMLTemplateElement} */
            var template = document.getElementById(templateId);

            var inputElementId = `parameter-editor-${uniqueId()}`;

            this.element = document.importNode(template.content, true);

            let labelElement = this.element.querySelector("label");
            labelElement.htmlFor = inputElementId;
            labelElement.textContent = parameter.displayName;

            this.inputElement = this.element.querySelector("[data-isinputelement]");
            this.inputElement.id = inputElementId;
            this.inputElement.title = parameter.displayName;

            initializeElementCallback(this.inputElement);
        }

        get value() { return ""; /* Should be overridden */ }
        set value(x) { /* Should be overridden */ }

        get isLanguageOutput() {
            return this.parameter.name === "OutputLanguage";
        }
    }

    class TextBoxParameterEditor extends ParameterEditor {
        /** @param {ShaderCompilerParameter} parameter */
        constructor(parameter) {
            super(
                parameter,
                "textbox-parameter-template",
                x => x.defaultValue = parameter.defaultValue);
        }

        get value() {
            return this.inputElement.value;
        }

        set value(x) {
            this.inputElement.value = x;
        }
    }

    class ComboBoxParameterEditor extends ParameterEditor {
        /** @param {ShaderCompilerParameter} parameter */
        constructor(parameter) {
            super(
                parameter,
                "combobox-parameter-template",
                x => {
                    for (let option of parameter.options) {
                        let isSelected = (option === parameter.defaultValue);
                        x.options.add(new Option(option, option, isSelected, isSelected));
                    }
                });
        }
        
        get value() {
            return this.inputElement.value;
        }

        set value(x) {
            this.inputElement.value = x;
        }
    }

    class CheckBoxParameterEditor extends ParameterEditor {
        /** @param {ShaderCompilerParameter} parameter */
        constructor(parameter) {
            super(
                parameter,
                "checkbox-parameter-template",
                x => x.checked = (parameter.defaultValue === "true"));
        }

        get value() {
            return this.inputElement.checked ? "true" : "false";
        }

        set value(x) {
            this.inputElement.checked = (x === "true");
        }
    }

    /** @type {ShaderLanguage[]} */
    var shaderLanguages = window.ShaderLanguages;

    /** @type {ShaderCompiler[]} */
    var shaderCompilers = window.ShaderCompilers;

    /** @type {CodeMirror.Editor} */
    var outputEditor = null;

    const codeMirrorTheme = "duotone-light";

    var codeEditor = CodeMirror.fromTextArea(document.getElementById('code'), {
        mode: "x-shader/hlsl",
        theme: codeMirrorTheme,
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4
    });

    /** @type {HTMLSelectElement} */
    var languageSelect = document.getElementById('language');

    for (var language of shaderLanguages) {
        languageSelect.options.add(new Option(language.name, language.name));
    }

    /** @type {HTMLSelectElement} */
    var outputStepsSelect = document.getElementById('output-steps');
    var outputTabsSelect = document.getElementById('output-tabs');
    var outputContainerDiv = document.getElementById('output-container');

    function getSelectedLanguage() {
        return shaderLanguages.find(x => x.name === languageSelect.selectedOptions[0].value);
    }

    function getCodeMirrorMode(language) {
        switch (language) {
            case "DXBC":
                return "text/x-dxbc";

            case "DXIL":
                return "text/x-llvm-ir";

            case "ISPC":
                return "text/x-c";

            default:
                return "text/plain";
        }
    }

    function createJsonRequestObject() {
        var jsonObject = {
            language: getSelectedLanguage().name,
            code: codeEditor.getValue()
        };

        var compilationSteps = [];

        for (let compilerEditor of compilerEditors.compilerEditors) {
            var jsonArguments = {};
            for (var parameterEditor of compilerEditor.parameterEditors) {
                jsonArguments[parameterEditor.parameter.name] = parameterEditor.value;
            }

            compilationSteps.push({
                compiler: compilerEditor.selectedCompiler.name,
                arguments: jsonArguments
            });
        }

        jsonObject.compilationSteps = compilationSteps;

        return jsonObject;
    }

    function compileCode() {
        var jsonObject = createJsonRequestObject();

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
            success: function (responses) {
                finishLoading();

                function selectStep() {
                    var response = responses.results[outputStepsSelect.selectedIndex];

                    var downloadBinaryButton = document.getElementById('download-binary-button');
                    if (response.binaryOutput !== null) {
                        downloadBinaryButton.classList.remove('invisible');
                        downloadBinaryButton.href = `data:text/plain;base64,${response.binaryOutput}`;
                        downloadBinaryButton.download = 'shader-binary.o';
                    } else {
                        downloadBinaryButton.classList.add('invisible');
                    }

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

                    while (outputTabsSelect.options.length > 0) {
                        outputTabsSelect.options.remove(0);
                    }

                    var currentScrollY = (outputEditor !== null)
                        ? outputEditor.getScrollInfo().top
                        : null;

                    outputTabsSelect.onchange = () => {
                        var output = response.outputs.find(x => x.displayName === outputTabsSelect.selectedOptions[0].value);

                        outputContainerDiv.innerHTML = '';

                        if (output.language === "graphviz") {
                            const workerURL = '/lib/vizjs/full.render.js';
                            let viz = new Viz({ workerURL });

                            viz.renderSVGElement(output.value)
                                .then(function (element) {
                                    outputContainerDiv.appendChild(element);
                                    var panZoom = svgPanZoom(element, {
                                        controlIconsEnabled: true
                                    });

                                    function resizePanZoom() {
                                        if (element.parentElement === null) {
                                            return;
                                        }
                                        element.setAttribute('width', outputContainerDiv.offsetWidth);
                                        element.setAttribute('height', outputContainerDiv.offsetHeight);
                                        panZoom.resize();
                                        panZoom.fit();
                                        panZoom.center();
                                    }

                                    resizePanZoom();

                                    $(window).resize(resizePanZoom);
                                })
                                .catch(error => {
                                    outputContainerDiv.innerText = 'Could not create graph: ' + error;
                                });
                        } else {
                            outputEditor = CodeMirror(outputContainerDiv, {
                                value: output.value,
                                mode: getCodeMirrorMode(output.language),
                                theme: codeMirrorTheme,
                                matchBrackets: true,
                                readOnly: true
                            });

                            if (currentScrollY !== null) {
                                outputEditor.scrollTo(null, currentScrollY);
                                currentScrollY = null;
                            }
                        }
                    };

                    for (var i = 0; i < response.outputs.length; i++) {
                        var output = response.outputs[i];
                        var isSelected = i === selectedOutputTabIndex;
                        outputTabsSelect.options.add(new Option(output.displayName, output.displayName, isSelected, isSelected));
                    }

                    outputTabsSelect.onchange();
                }

                while (outputStepsSelect.options.length > 0) {
                    outputStepsSelect.options.remove(0);
                }

                var alreadySelected = false;

                for (var i = responses.results.length - 1; i >= 0; i--) {
                    var isSelected = !alreadySelected && responses.results[i].success;
                    if (isSelected) {
                        alreadySelected = true;
                    }
                    var compilerEditor = compilerEditors.compilerEditors[i];
                    outputStepsSelect.options.add(
                        new Option(compilerEditor.fullDisplayName, compilerEditor.fullDisplayName, isSelected, isSelected),
                        0);
                }

                outputStepsSelect.onchange = selectStep;

                outputStepsSelect.onchange();
            }
        });
    }

    var compileCodeTimeout;
    var initialized = false;

    function somethingChanged(trigger) {
        if (!initialized) {
            return;
        }

        if (window.location.search !== "") {
            window.history.pushState(null, null, window.location.origin + window.location.pathname);
        }

        clearTimeout(compileCodeTimeout);
        compileCodeTimeout = setTimeout(compileCode, 500);
    }

    codeEditor.on('change', () => somethingChanged());

    /** @param {HTMLSelectElement} element - */
    function raiseSelectElementChange(element) {
        var event = document.createEvent('Event');
        event.initEvent('change');
        element.dispatchEvent(event);
    }

    class CompilerEditorCollection {
        constructor() {
            this.element = document.getElementById("compiler-editors");

            /** @type {CompilerEditor[]} */
            this.compilerEditors = [];
        }

        getPrevious(compilerEditor) {
            var index = this.compilerEditors.indexOf(compilerEditor);
            return (index !== 0)
                ? this.compilerEditors[index - 1]
                : null;
        }

        removeAfter(compilerEditor) {
            this._removeFrom(this.compilerEditors.indexOf(compilerEditor) + 1);
        }

        removeFrom(compilerEditor) {
            this._removeFrom(this.compilerEditors.indexOf(compilerEditor));
        }

        _removeFrom(index) {
            for (let i = index; i < this.compilerEditors.length; i++) {
                this.element.removeChild(this.compilerEditors[i].element);
            }
            this.compilerEditors.splice(index);
            this.updateAddButton();
        }

        reset() {
            this._removeFrom(0);
            this.add(getSelectedLanguage());
        }

        applyState(compilationSteps) {
            this._removeFrom(0);

            let inputLanguage = getSelectedLanguage();
            for (const compilationStep of compilationSteps) {
                const compilerEditor = this.add(inputLanguage);

                compilerEditor.compilerSelect.value = compilationStep.compiler;
                compilerEditor.onCompilerChanged();

                for (const argumentName in compilationStep.arguments) {
                    const parameterEditor = compilerEditor.parameterEditors.find(x => x.parameter.name === argumentName);
                    parameterEditor.value = compilationStep.arguments[argumentName];
                }

                const inputLanguageName = compilerEditor.outputLanguage;
                if (inputLanguageName !== null) {
                    inputLanguage = { name: inputLanguageName };
                }
            }
        }

        add(language) {
            const compilerEditor = new CompilerEditor(language);
            this.element.appendChild(compilerEditor.element);
            this.compilerEditors.push(compilerEditor);

            compilerEditor.element.querySelector('.lead').innerText = compilerEditor.displayName;

            compilerEditor.onCompilerChanged();

            this.updateAddButton();

            return compilerEditor;
        }

        updateAddButton() {
            var addCompilerButtonContainer = document.getElementById('add-compiler-button-container');
            var addCompilerButton = document.getElementById('add-compiler-button');

            addCompilerButtonContainer.classList.add('d-none');

            if (this.compilerEditors.length === 0) {
                return;
            }

            // If last compiler has an output that can be accepted as an input by a compiler,
            // show the Add Compiler button.
            let lastCompiler = this.compilerEditors[this.compilerEditors.length - 1];
            let outputLanguage = lastCompiler.outputLanguage;
            if (outputLanguage !== null) {
                let compiler = shaderCompilers.find(x => x.inputLanguages.includes(outputLanguage));
                if (compiler !== undefined) {
                    addCompilerButtonContainer.classList.remove('d-none');
                    addCompilerButton.onclick = () => {
                        compilerEditors.add({ name: outputLanguage });
                        return false;
                    };
                }
            }
        }
    }

    let compilerEditors = new CompilerEditorCollection();

    class CompilerEditor {
        /**
         * @param {ShaderLanguage} inputLanguage
         */
        constructor(inputLanguage) {
            /** @type {HTMLTemplateElement} */
            let template = document.getElementById("compiler-editor-template");

            let element = document.importNode(template.content, true).querySelector(".compiler-container");

            let compilerSelectId = `compiler-select-${uniqueId()}`;
            element.querySelector("label").htmlFor = compilerSelectId;

            element.querySelector("[data-remove-link]").onclick = () => {
                compilerEditors.removeFrom(this);
                somethingChanged();
                return false;
            };

            let compilerSelect = element.querySelector("select");
            compilerSelect.id = compilerSelectId;
            for (let compiler of shaderCompilers) {
                if (compiler.inputLanguages.includes(inputLanguage.name)) {
                    compilerSelect.options.add(new Option(compiler.displayName, compiler.name));
                }
            }
            this.compilerSelect = compilerSelect;

            this.descriptionSpan = element.querySelector("span");
            this.cardBody = element.querySelector(".card-body");
            this.argumentsDiv = element.querySelector("[data-arguments]");

            /** @type {ParameterEditor[]} */
            this.parameterEditors = [];

            compilerSelect.addEventListener("change", () => this.onCompilerChanged());

            this.element = element;
        }

        get selectedCompiler() {
            return shaderCompilers.find(x => x.name === this.compilerSelect.selectedOptions[0].value);
        }

        onCompilerChanged() {
            let compiler = this.selectedCompiler;

            this.argumentsDiv.innerHTML = '';
            this.parameterEditors.length = 0;

            if (compiler.parameters.length == 0) {
                this.cardBody.classList.add('d-none');
            } else {
                this.cardBody.classList.remove('d-none');
            }

            for (let parameter of compiler.parameters) {
                let parameterEditor = ParameterEditor.create(parameter);

                parameterEditor.inputElement.addEventListener(
                    'input',
                    () => {
                        if (parameterEditor.isLanguageOutput) {
                            compilerEditors.removeAfter(this);
                        }
                        somethingChanged();
                    });

                this.parameterEditors.push(parameterEditor);
                this.argumentsDiv.appendChild(parameterEditor.element);
            }

            this.descriptionSpan.textContent = compiler.description;

            // If this is not the first compiler, set its parameter values to match those of the previous compiler.
            let previousCompiler = compilerEditors.getPrevious(this);
            if (previousCompiler !== null) {
                for (let parameterEditor of this.parameterEditors) {
                    if (parameterEditor.isLanguageOutput) {
                        continue;
                    }
                    let previousParameterEditor = previousCompiler.parameterEditors.find(x => x.parameter.name === parameterEditor.parameter.name);
                    if (previousParameterEditor !== undefined) {
                        parameterEditor.value = previousParameterEditor.value;
                    }
                }
            }

            compilerEditors.removeAfter(this);
            somethingChanged();
        }

        get outputLanguage() {
            let languageOutputParameter = this.parameterEditors.find(x => x.isLanguageOutput);
            return (languageOutputParameter !== undefined)
                ? languageOutputParameter.value
                : null;
        }

        get displayName() {
            return `Compiler #${(compilerEditors.compilerEditors.indexOf(this) + 1)}`;
        }

        get fullDisplayName() {
            return `#${(compilerEditors.compilerEditors.indexOf(this) + 1)} - ${this.selectedCompiler.displayName}`;
        }
    }

    function loadFromUrl() {
        if (window.location.pathname.length < 2) {
            return false;
        }

        var gistId = window.location.pathname.substring(1);

        $.ajax({
            url: `https://api.github.com/gists/${gistId}`,
            type: "GET",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                alert("Unexpected error loading gist");
            },
            success: function (response) {
                const configJson = JSON.parse(response.files["config.json"].content);

                const shaderLanguage = shaderLanguages.find(x => x.name === configJson.language);
                const fileExtension = shaderLanguage.fileExtension;

                const code = response.files[`shader.${fileExtension}`].content;
                codeEditor.setValue(code);

                languageSelect.value = configJson.language;
                compilerEditors.applyState(configJson.compilationSteps);

                compileCode();

                initialized = true;
            }
        });

        return true;
    }

    languageSelect.addEventListener(
        "change",
        () => {
            codeEditor.setValue(getSelectedLanguage().defaultCode);
            compilerEditors.reset();
            somethingChanged();
        });

    if (!loadFromUrl()) {
        initialized = true;
        raiseSelectElementChange(languageSelect);
    }

    /** @type {HTMLInputElement} */
    let permalinkTextbox = document.getElementById("permalink-textbox");

    /** @type {HTMLButtonElement} */
    let copyPermalinkButton = document.getElementById('copy-permalink-button');

    document.getElementById("share-button").onclick = () => {
        var jsonObject = createJsonRequestObject();

        permalinkTextbox.value = '';
        permalinkTextbox.placeholder = 'Loading...';
        copyPermalinkButton.textContent = 'Copy';

        function finishLoading() {
            permalinkTextbox.placeholder = '';
        }

        $.ajax({
            url: window.CreateGistUrl,
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
                permalinkTextbox.value = `${window.location.origin}/${response}`;
                permalinkTextbox.select();
                window.history.pushState(null, null, permalinkTextbox.value);
            }
        });

        $('#share-dialog').modal({});

        return false;
    };

    document.getElementById('copy-permalink-button').onclick = () => {
        permalinkTextbox.select();
        var copied = false;
        try {
            copied = document.execCommand('copy');
        } catch (err) {

        }

        copyPermalinkButton.textContent = copied ? "Copied" : "Couldn't copy";
        
        return false;
    };

    document.getElementById("changelog-button").onclick = () => {
        document.getElementById('changelog-content').innerText = 'Loading changelog...';

        $.get('https://raw.githubusercontent.com/tgjones/shader-playground/master/CHANGELOG.md', data => {
            var requestObject = {
                text: data,
                mode: 'gfm',
                context: 'tgjones/shader-playground'
            };

            $.ajax({
                url: `https://api.github.com/markdown`,
                type: "POST",
                data: JSON.stringify(requestObject),
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error: function (response) {
                    document.getElementById('changelog-content').innerHTML = response.responseText;
                },
                success: function (response) {
                    document.getElementById('changelog-content').innerHTML = response.responseText;
                }
            });
        });

        $('#changelog-dialog').modal({});

        return false;
    };
});