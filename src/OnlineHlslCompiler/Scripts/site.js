$(function () {
    function refreshTargetProfileOptions() {
        var selectedCompiler = $("#Compiler").val();
        var targetProfiles = allTargetProfiles[selectedCompiler];

        var targetProfileSelect = $("#TargetProfile");
        targetProfileSelect.empty();

        for (var i = 0; i < targetProfiles.length; i++) {
            var targetProfile = targetProfiles[i];
            var option = $('<option></option>').
                attr("value", targetProfile[0]).
                text(targetProfile[1]);
            if (targetProfile[1].startsWith("ps_"))
                option = option.attr("selected", true);
            targetProfileSelect.append(option);
        }
    }

    refreshTargetProfileOptions();

    $("#Compiler").change(refreshTargetProfileOptions);

    var codeEditor = CodeMirror(document.getElementById('code-editor-container'), {
        value: $("#Code").val(),
        mode: "x-shader/hlsl",
        theme: "neat",
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4
    });

    var disassemblyEditor = CodeMirror(document.getElementById('disassembly-container'), {
        mode: "text/x-llvm-ir",
        theme: "neat",
        matchBrackets: true,
        styleActiveLine: true,
        readOnly: true
    });

    function compileCode() {
        var jsonObject = {
            "Code": codeEditor.getValue(),
            "Compiler": $("#Compiler").val(),
            "TargetProfile": $("#TargetProfile").val(),
            "EntryPointName": $("#EntryPointName").val()
        };

        $(".results").addClass("loading");

        var spinner = new Spinner({ color: '#AAAAAA', scale: 0.5 }).spin();
        $(".loader").append(spinner.el);
        $(".loader").show();

        function finishLoading() {
            spinner.stop();
            $(".loader").hide();
            $(".results").removeClass("loading");
        }

        $.ajax({
            url: $(document.body).data("compile-url"),
            type: "POST",
            data: JSON.stringify(jsonObject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                finishLoading();
                alert(response.responseText);
            },
            success: function (response) {
                finishLoading();
                if (response.HasErrors) {
                    $("#compiler-errors").text(response.Message);
                    $(".decompiled").hide();
                    $(".errors").show();
                } else {
                    disassemblyEditor.setValue(response.Disassembly);
                    $(".errors").hide();
                    $(".decompiled").show();
                }
            }
        });
    }

    compileCode();

    var compileCodeTimeout;

    function somethingChanged() {
        clearTimeout(compileCodeTimeout);
        compileCodeTimeout = setTimeout(compileCode, 500);
    }

    codeEditor.on('change', somethingChanged);
    $("#Compiler").change(somethingChanged);
    $("#TargetProfile").change(somethingChanged);
    $("#EntryPointName").on('input propertychange paste', somethingChanged);
});