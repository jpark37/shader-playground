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

    var myCodeMirror = CodeMirror(document.getElementById('code-editor-container'), {
        value: $("#Code").val(),
        mode: "text/x-c++src",
        theme: "neat",
        lineNumbers: true,
        matchBrackets: true,
        styleActiveLine: true,
        indentUnit: 4
    });

    function compileCode() {
        var jsonObject = {
            "Code": myCodeMirror.getValue(),
            "Compiler": $("#Compiler").val(),
            "TargetProfile": $("#TargetProfile").val(),
            "EntryPointName": $("#EntryPointName").val()
        };

        $.ajax({
            url: $(document.body).data("compile-url"),
            type: "POST",
            data: JSON.stringify(jsonObject),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            error: function (response) {
                alert(response.responseText);
            },
            success: function (response) {
                $("#disassemblyDiv").removeClass("error");
                if (response.HasErrors) {
                    $("#disassemblyDiv").text(response.Message);
                    $("#disassemblyDiv").addClass("error");
                } else {
                    $("#disassemblyDiv").text(response.Disassembly);
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

    myCodeMirror.on('change', somethingChanged);
    $("#Compiler").change(somethingChanged);
    $("#TargetProfile").change(somethingChanged);
    $("#EntryPointName").on('input propertychange paste', somethingChanged);
});