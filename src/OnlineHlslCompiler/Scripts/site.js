$(function () {
    $('#compileBtn').click(function (e) {
        var jsonObject = {
            "Code" : $("#Code").val(),
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