var _ajax = {
    call: function(type, url, data, callback, isAlert) {
        $.ajax({
            type: type,
            timeout: 30000,
            url: url,
            data: data,
            cache: false,
            async: false,
            dataType: 'json',
            success: function(response) {
                if (response.Result == 'success') {
                    if (typeof callback === "function") {
                        callback(response);
                    }
                } else {
                    this.error(response);
                }
            },
            error: function(response) {
                if (response.responseText != null) {
                    response = $.common.parseJson(response.responseText);
                }

                $.each(response.ErrorList, function(index, element) {
                    if (element.Point == 'errorMessage') {
                        if (isAlert) {
                            alert('エラー！ ' + element.Message);
                        }
                        callback(response);
                    }
                });
            },
            complete: function() {
            }
        });
    }
}