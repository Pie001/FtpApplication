﻿'use strict';

$(function() {
    _loading.init();
    _connectionConfig.init();
    _commonModal.init();
    _copyHostModal.init();
});

var _loading = {
    $me: null,
    init: function() {
        this.$me = $('.loading');
    }
}

var _copyHostModal = {
    $me: null,
    init: function() {
        this.$me = $('#modal_copy_host');
        this.form.init();
        this.btnOK.init();
        this.masterHostName.init();
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_copy_host');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        },
        isValid: function() {
            return this.$me.validate().form();
        }
    },
    masterHostName:{
        $me:null,
        init:function(){
            this.$me = $('.master-host-name');
        }
    },
    show: function() {
        _copyHostModal.masterHostName.$me.empty();
        
        _copyHostModal.masterHostName.$me.append('ホスト名「' + _connectionConfig.hostMain.listBlock.getHostItemName(_connectionConfig.selectedHostId) + '」をコピーします。');
        

        this.$me.find('input[name="NewHostName"]').val('');
        this.$me.modal('show');
    },
    hide: function() {
        this.$me.modal('hide');
    },
    btnOK: {
        $me: null,
        init: function() {
            this.$me = $('#btn_copy_host_submit');
            this.click();
        },
        click: function() {
            this.$me.click(function(event) {
                _loading.$me.fadeIn();
                var param = _copyHostModal.form.getSerializeArray();
                param.push({ name: 'selectHostId', value: _connectionConfig.selectedHostId });
                _ajax.call('POST', '/BGHost/CopyHost', param, function(response) {
                    if (response != null && response.Result == 'success') {
                        _connectionConfig.hostMain.listBlock.addNewHostItem(response);

                        if (typeof _ftpControlBlock != 'undefined') {
                            _ftpControlBlock.hostMenuBlock.reset();
                        }
                        $('.host-main').animate({scrollTop: 0}, 400);

                        _copyHostModal.hide();
                        _loading.$me.fadeOut();
                    }
                }, true);

                return false;
            });
        }
    }
}

var _commonModal = {
    $me: null,
    isNew: false,
    init: function() {
        this.$me = $('#modal_edit_host');
        this.form.init();
        this.btnOk.init();
        this.btnConnectTest.init();
        this.connectTestResult.init();
        this.title.init();
        this.hostSettiesBlock.init();
        this.btnHostSettings.init();
        this.inputHostName.init();
        this.inputHostAddress.init();
        this.inputPortNumber.init();
        this.inputUserName.init();
        this.inputPassword.init();
        this.inputTimeout.init();
        this.inputAnonymous.init();
        this.inputPasvMode.init();
        this.inputLocalServerPath.init();
        this.inputRemoteServerPath.init();
    },
    resetForm: function(isNew) {
        $('input[class="input-validation-error"]').val('');
        $('input').removeClass('input-validation-error');
        $('span[class="field-validation-error"]').empty();

        _commonModal.title.$me.empty();
        _commonModal.title.$me.prepend('<i class="icon-plus-3"></i> ' + (isNew ? 'New' : 'Edit'));

        _commonModal.connectTestResult.$me.css('display','none');
        _commonModal.connectTestResult.$me.empty();

        this.hostSettiesBlock.hide();
        this.btnHostSettings.changeIconClass('icon-down-open-3', 'icon-right-open-4');
    },
    show: function(isNew) {
        this.isNew = isNew;
        this.resetForm(isNew);
        this.$me.modal('show');
    },
    hide: function() {
        this.$me.modal('hide');
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_edit_host');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        },
        isValid: function() {
            return this.$me.validate().form();
        }
    },
    btnOk: {
        $me: null,
        init: function() {
            this.$me = $('#btn_edit_ok');
            this.click();
        },
        click: function() {
            this.$me.click(function(event) {
                var isValid = _commonModal.form.isValid();
                var isSuccess = false;
                if (isValid) {
                    _loading.$me.fadeIn();
                    var param = _commonModal.form.getSerializeArray();
                    if (_commonModal.isNew) {
                        _ajax.call('POST', '/BGHost/AddHost', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                _connectionConfig.hostMain.listBlock.addNewHostItem(response);
                                isSuccess = true;
                            }
                        }, true);
                    } else {
                        param.push({ name: 'HostID', value: _connectionConfig.selectedHostId });
                        _ajax.call('POST', '/BGHost/EditHost', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                _connectionConfig.hostMain.listBlock.editHostItem(response);
                                isSuccess = true;
                            }
                        }, true);
                    }

                    if (isSuccess) {
                        if (typeof _ftpControlBlock != 'undefined') {
                            _ftpControlBlock.hostMenuBlock.reset();
                        }
                        if (_commonModal.isNew) {
                            $('.host-main').animate({ scrollTop: 0 }, 400);
                        }

                        _commonModal.hide();
                        _loading.$me.fadeOut();
                    }
                }
                return false;
            });
        }
    },
    btnConnectTest: {
        $me: null,
        init: function() {
            this.$me = $('#btn_connect_test');
            this.click();
        },
        click: function() {
            this.$me.click(function(event) {
                var isValid = _commonModal.form.isValid();
                if (isValid) {
                    _loading.$me.fadeIn();
                    var param = _commonModal.form.getSerializeArray();
                    _ajax.call('POST', '/BGHost/ConnectTest', param, function(response) {
                        if (response != null && response.Result == 'success') {
                            _commonModal.connectTestResult.$me.empty();
                            _commonModal.connectTestResult.$me.prepend('<span>【OK】Connection parameters are correct.</span>');
                            _commonModal.connectTestResult.$me.css('display', 'block');
                        } else {
                            _commonModal.connectTestResult.$me.empty();
                            _commonModal.connectTestResult.$me.prepend('<span class="connect-test-ng">【NG】Faild to Connect to FTP.</span>');
                            _commonModal.connectTestResult.$me.css('display', 'block');
                        }
                        
                    }, false);
                    _loading.$me.fadeOut();
                }
            });
        }
    },
    connectTestResult: {
        $me: null,
        init: function() {
            this.$me = $('.connect-test-result');
        }
    },
    title: {
        $me: null,
        init: function() {
            this.$me = _commonModal.$me.find('div.modal-header label.title');
        }
    },
    hostSettiesBlock: {
        $me: null,
        init: function() {
            this.$me = $('#host_add_option');
        },
        hide: function() {
            this.$me.hide();
        }
    },
    btnHostSettings: {
        $me: null,
        init: function() {
            this.$me = $('#btn_host_settings');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                var icon_right = 'icon-right-open-4',
                    icon_down = 'icon-down-open-3';
                var $iconHostSettings = _commonModal.btnHostSettings.$me.find('i:first');

                if ($iconHostSettings.hasClass(icon_right)) {
                    _commonModal.hostSettiesBlock.$me.slideDown();

                    $iconHostSettings.removeClass(icon_right);
                    $iconHostSettings.addClass(icon_down);
                } else {
                    _commonModal.hostSettiesBlock.$me.slideUp();

                    $iconHostSettings.removeClass(icon_down);
                    $iconHostSettings.addClass(icon_right);
                }

                return false;
            });
        },
        changeIconClass: function(before, after) {
            var $iconHostSettings = this.$me.find('i:first');
            if ($iconHostSettings.hasClass(before)) {
                $iconHostSettings.removeClass(before);
                $iconHostSettings.addClass(after);
            }
        }
    },
    inputHostName: {
        $me: null,
        init: function() {
            this.$me = $('#HostName');
        }
    },
    inputHostAddress: {
        $me: null,
        init: function() {
            this.$me = $('#HostAddress');
        }
    },
    inputPortNumber: {
        $me: null,
        init: function() {
            this.$me = $('#PortNumber');
        }
    },
    inputUserName: {
        $me: null,
        init: function() {
            this.$me = $('#UserName');
        }
    },
    inputPassword: {
        $me: null,
        init: function() {
            this.$me = $('#Password');
        }
    },
    inputTimeout: {
        $me: null,
        init: function() {
            this.$me = $('#Timeout');
        }
    },
    inputAnonymous: {
        $me: null,
        init: function() {
            this.$me = $('#Anonymous');
        }
    },
    inputPasvMode: {
        $me: null,
        init: function() {
            this.$me = $('#PasvMode');
        }
    },
    inputLocalServerPath: {
        $me: null,
        init: function() {
            this.$me = $('#LocalServerPath');
        }
    },
    inputRemoteServerPath: {
        $me: null,
        init: function() {
            this.$me = $('#RemoteServerPath');
        }
    },
    resetInputValues: function() {
        _commonModal.inputHostName.$me.val('');
        _commonModal.inputHostAddress.$me.val('');

        _commonModal.inputPortNumber.$me.val('21');
        _commonModal.inputUserName.$me.val('');
        _commonModal.inputPassword.$me.val('');
        _commonModal.inputTimeout.$me.val('6000');

        _commonModal.inputAnonymous.$me.attr('checked', false);
        _commonModal.inputPasvMode.$me.attr('checked', true);

        _commonModal.inputLocalServerPath.$me.val('');
        _commonModal.inputRemoteServerPath.$me.val('');

        _connectionConfig.hostMain.listBlock.clickHostItem();
    },
    setInputValues: function(response) {
        _commonModal.inputHostName.$me.val(response.ResponseData.hostName);
        _commonModal.inputHostAddress.$me.val(response.ResponseData.hostAddress);

        _commonModal.inputPortNumber.$me.val(response.ResponseData.portNumber);
        _commonModal.inputUserName.$me.val(response.ResponseData.userName);
        _commonModal.inputPassword.$me.val(response.ResponseData.password);
        _commonModal.inputTimeout.$me.val(response.ResponseData.timeout);

        _commonModal.inputLocalServerPath.$me.val(response.ResponseData.localServerPath);
        _commonModal.inputRemoteServerPath.$me.val(response.ResponseData.remoteServerPath);

        if (response.ResponseData.anonymous == 'True') {
            _commonModal.inputAnonymous.$me.attr('checked', true);
        } else {
            _commonModal.inputAnonymous.$me.attr('checked', false);
        }

        if (response.ResponseData.pasvMode == 'True') {
            _commonModal.inputPasvMode.$me.attr('checked', true);
        } else {
            _commonModal.inputPasvMode.$me.attr('checked', false);
        }

        _connectionConfig.hostMain.listBlock.clickHostItem();
    }
}

var _connectionConfig = {
    $me: null,
    selectedHostId: null,
    init: function() {
        this.$me = $('#block_conection_config');
        this.form.init();
        this.hostMain.init();
        this.btnNewHost.init();
        this.btnEditHost.init();
        this.btnCopyHost.init();
        this.btnRemoveHost.init();
        this.btnConnect.init();
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_host_settings');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        }
    },
    hostMain: {
        $me: null,
        init: function() {
            this.$me = $('.host-main');
            this.listBlock.init();
        },
        listBlock: {
            $me: null,
            init: function() {
                this.$me = $('#host_list_block');
                this.clickHostItem();
            },
            clickHostItem: function() {
                var $selectItem = this.$me.find('li');
                $selectItem.click(function() {
                    var index = $selectItem.index(this);

                    $selectItem.removeClass('host-list-select');
                    $selectItem.eq(index).addClass('host-list-select');

                    var hostId = $selectItem.eq(index).attr('data-hostid');
                    _connectionConfig.selectedHostId = hostId;
                    _connectionConfig.activateAllBtn(true);

                    return false;
                });

                this.$me.sortable({
                    update: function() {
                        var hostList = [];
                        var hostId;
                        var sortNumber = 0;
                        $.each(_connectionConfig.hostMain.listBlock.$me.find('li'), function(index, element) {
                            hostId = $(element).attr('data-hostid');
                            var data = [hostId, sortNumber];
                            hostList.push(data);
                            sortNumber++;
                        });

                        var param = _connectionConfig.form.getSerializeArray();
                        param.push({ name: 'hostList', value: hostList.join(':') });

                        _ajax.call('POST', '/BGHost/SortHost', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                if (typeof _ftpControlBlock != 'undefined') {
                                    _ftpControlBlock.hostMenuBlock.reset();
                                }
                            }
                        }, true);
                    }
                });
                this.$me.disableSelection();
            },
            checkHostItem: function(hostId) {
                if (hostId == '' && hostId != null) {
                    return false;
                }
                var hostName = this.$me.find('li[data-hostid=' + hostId + ']').text();

                if (hostName != '' && hostName != null) {
                    return true;
                } else {
                    return false;
                }
            },
            getHostItemName: function(hostId) {
                return this.$me.find('li[data-hostid=' + hostId + ']').text();
            },
            addNewHostItem: function(response) {
                $.each(_connectionConfig.hostMain.listBlock.$me.find('li'), function(index, element) {
                    $(element).removeClass('host-list-select');
                });
                this.$me.prepend('<li data-hostid="' + response.ResponseData.hostId + '" class="host-list-select"><i class="icon-monitor"></i> ' + response.ResponseData.hostName + '</li>');
                _connectionConfig.selectedHostId = response.ResponseData.hostId;
                _connectionConfig.activateAllBtn(true);

                _connectionConfig.hostMain.listBlock.clickHostItem();
            },
            editHostItem: function(response) {
                var $selectItem = _connectionConfig.hostMain.listBlock.getHostItemObject(response.ResponseData.hostId);
                $selectItem.empty();
                $selectItem.append('<i class="icon-monitor"></i> ' + response.ResponseData.hostName);

                _connectionConfig.hostMain.listBlock.clickHostItem();
            },
            getHostItemObject: function(hostId) {
                return this.$me.find('li[data-hostid=' + hostId + ']');
            },
            getHostItemValues: function(hostId) {
                var response;
                _ajax.call('GET', '/BGHost/HostItem', 'hostId=' + hostId, function(json) {
                    if (json != null && json.Result == 'success') {
                        response = json;
                    }
                }, true);
                return response;
            }
        }
    },
    btnNewHost: {
        $me: null,
        init: function() {
            this.$me = $('#btn_new_host');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                _commonModal.resetInputValues();
                _commonModal.show(true);

                return false;
            });
        },
        active: function(isActive) {
            if (isActive) {
                this.$me.removeAttr('disabled');
            } else {
                this.$me.attr('disabled', true);
            }
        }
    },
    btnEditHost: {
        $me: null,
        init: function() {
            this.$me = $('#btn_edit_host');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                var hostItemValues = _connectionConfig.hostMain.listBlock.getHostItemValues(_connectionConfig.selectedHostId);
                _commonModal.setInputValues(hostItemValues);
                _commonModal.show(false);

                return false;
            });
        },
        active: function(isActive) {
            if (isActive) {
                this.$me.removeAttr('disabled');
            } else {
                this.$me.attr('disabled', true);
            }
        }
    },
    btnCopyHost: {
        $me: null,
        init: function() {
            this.$me = $('#btn_copy_host');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                var hostId = _connectionConfig.selectedHostId;

                if (!_connectionConfig.hostMain.listBlock.checkHostItem(hostId)) {
                    alert('コピーするホストを正しく選択してください！');
                } else {
                    _copyHostModal.show();
                }
                return false;
            });
        },
        success: function(response) {
            if (response != null) {
                _connectionConfig.hostMain.listBlock.addNewHostItem(response);
            }
        },
        active: function(isActive) {
            if (isActive) {
                this.$me.removeAttr('disabled');
            } else {
                this.$me.attr('disabled', true);
            }
        }
    },
    btnRemoveHost: {
        $me: null,
        init: function() {
            this.$me = $('#btn_remove_host');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                var hostId = _connectionConfig.selectedHostId;

                if (!_connectionConfig.hostMain.listBlock.checkHostItem(hostId)) {
                    alert('削除ホストを正しく選択してください！');
                } else {
                    if (confirm('ホスト「' + _connectionConfig.hostMain.listBlock.getHostItemName(hostId) + '」を削除します')) {
                        _loading.$me.fadeIn();
                        var param = _connectionConfig.form.getSerializeArray();
                        param.push({ name: 'selectHostId', value: hostId });

                        _ajax.call('POST', '/BGHost/RemoveHost', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                var hostId = response.ResponseData.hostId;
                                _connectionConfig.hostMain.listBlock.$me.find('li[data-hostid=\"' + hostId + '\"]').remove();
                                _connectionConfig.reset();

                                if (typeof _ftpControlBlock != 'undefined') {
                                    _ftpControlBlock.hostMenuBlock.reset();
                                }
                            }
                        }, true);
                        _loading.$me.fadeOut();
                    }
                }

                return false;
            });
        },
        active: function(isActive) {
            if (isActive) {
                this.$me.removeAttr('disabled');
            } else {
                this.$me.attr('disabled', true);
            }
        }
    },
    reset: function() {
        _connectionConfig.selectedHostId = null;
        _connectionConfig.activateAllBtn(false);
    },
    activateAllBtn: function(isActive) {
        if (isActive) {
            _connectionConfig.btnNewHost.active(isActive);
        }
        _connectionConfig.btnEditHost.active(isActive);
        _connectionConfig.btnCopyHost.active(isActive);
        _connectionConfig.btnRemoveHost.active(isActive);
        _connectionConfig.btnConnect.active(isActive);
    },
    btnConnect: {
        $me: null,
        init: function() {
            this.$me = $('#btn_connect');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                var param = _connectionConfig.form.getSerializeArray();
                param.push({ name: 'selectHostId', value: _connectionConfig.selectedHostId });
                _ajax.call('POST', '/BGFTP/Connect', param, function(response) {
                    if (response != null && response.Result == 'success') {
                        window.location.href = '/FTP/';
                    }
                }, true);
                return false;
            })
        },
        active: function(isActive) {
            if (isActive) {
                this.$me.removeAttr('disabled');
            } else {
                this.$me.attr('disabled', true);
            }
        }
    }
};