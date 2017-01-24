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

    $('#compileBtn').click(function (e) {
        var jsonObject = {
            "Code": $("#Code").val(),
            "Compiler": $("#Compiler").val(),
            "TargetProfile": $("#TargetProfile").val(),
            "EntryPointName": $("#EntryPointName").val()
        };

        $.ajax({
            url: $("#compileBtn").data("compile-url"),
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

        return false;
    });
});