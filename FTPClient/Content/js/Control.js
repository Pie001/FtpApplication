'use strict';

$(function() {
    _ftpControlBlock.init();
    _hostOptionModal.init();
    _makeDirectoryModal.init();
    _changeResourceNameModal.init();
    _ftpControlBlock.operationLogBlock.log.scollTop();

    $(document).click(function() {
        _ftpControlBlock.server.local.contextMenu.hide();
        _ftpControlBlock.server.remote.contextMenu.hide();
        _ftpControlBlock.btnInit();
    });
});

// ============== ホスト関連メニュー ==============

var _hostOptionModal = {
    $me: null,
    isShow:false,
    init: function() {
        this.$me = $('#modal_host_option');
        this.isHide();
    },
    show: function() {
        this.$me.modal('show');
        this.isShow = true;
    },
    isHide: function() {
        _hostOptionModal.$me.on('hidden.bs.modal', function() {
            _hostOptionModal.isShow = false;
        })
    }
}

var _makeDirectoryModal = {
    $me: null,
    init: function() {
        this.$me = $('#modal_make_directory');
        this.form.init();
        this.directoryName.init();
        this.btnOK.init();
        this.title.init();
    },
    title: {
        $me: null,
        init: function() {
            this.$me = _makeDirectoryModal.$me.find('div.modal-header h4.title');
        }
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_make_directory');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        },
        isValid: function() {
            return this.$me.validate().form();
        }
    },
    directoryName: {
        $me: null,
        init: function() {
            this.$me = $('#DirectoryName');
        }
    },
    btnOK: {
        $me: null,
        init: function() {
            this.$me = $('#btn_make_directory_submit');
            this.click();
        },
        click: function() {
            this.$me.click(function(event) {
                if (_makeDirectoryModal.form.isValid()) {
                    _loading.$me.fadeIn();
                    var param = _makeDirectoryModal.form.getSerializeArray();
                    var selected = _ftpControlBlock.server.selectedServer;
                    param.push({ name: 'env', value: selected });

                    _ajax.call('POST', '/BGFTP/MakeDirectory', param, function(response) {
                        if (response != null && response.Result == 'success') {
                            _ftpControlBlock.server.reset(selected);

                            _makeDirectoryModal.hide();
                            _makeDirectoryModal.directoryName.$me.val('');
                        }
                    }, true);

                    _ftpControlBlock.operationLogBlock.reset();

                    _ftpControlBlock.server.local.contextMenu.hide();
                    _ftpControlBlock.server.remote.contextMenu.hide();
                    _ftpControlBlock.btnInit();

                    _loading.$me.fadeOut();
                }
                return false;
            });
        }
    },
    show: function() {
        var env = '';
        if (_ftpControlBlock.server.selectedServer == 'local') {
            env = 'ローカル';
        } else {
            env = 'リモート';
        }
        _makeDirectoryModal.title.$me.empty();
        _makeDirectoryModal.title.$me.prepend('<i class="icon-folder-add"></i> ディレクトリ作成(' + env + ')');

        this.$me.modal('show');
    },
    hide: function() {
        this.$me.modal('hide');
    }
}

var _changeResourceNameModal = {
    $me: null,
    beforeName: null,
    isDir: false,
    init: function() {
        this.$me = $('#modal_change_resource_name');
        this.form.init();
        this.newName.init();
        this.btnOK.init();
        this.title.init();
        this.currentFileInfo.init();
    },
    title: {
        $me: null,
        init: function() {
            this.$me = _changeResourceNameModal.$me.find('div.modal-header h4.title');
        }
    },
    currentFileInfo: {
        $me: null,
        init: function() {
            this.$me = _changeResourceNameModal.$me.find('div.modal-body p.current-file-info');
        }
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_change_resource_name');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        },
        isValid: function() {
            return this.$me.validate().form();
        }
    },
    newName: {
        $me: null,
        init: function() {
            this.$me = $('#NewName');
        }
    },
    show: function() {
        _changeResourceNameModal.title.$me.empty();
        var env = '';
        var beforeName = '';
        var isDir = false;

        var selected = _ftpControlBlock.server.selectedServer;
        if (selected == 'local') {
            env = 'ローカル';
        } else {
            env = 'リモート';
        }

        if (selected == 'local') {
            $.each(_ftpControlBlock.server.local.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                beforeName = $(element).attr('data-fileid');
                if ($(element).find('i').hasClass('icon-folder-1')) {
                    isDir = true;
                }
            });
        } else {
            $.each(_ftpControlBlock.server.remote.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                beforeName = $(element).attr('data-fileid');
                if ($(element).find('i').hasClass('icon-folder-1')) {
                    isDir = true;
                }
            });
        }

        if (beforeName != '') {
            _changeResourceNameModal.title.$me.empty();
            _changeResourceNameModal.title.$me.prepend('<i class="icon-tags"></i> 名前変更(' + env + ')');
            _changeResourceNameModal.currentFileInfo.$me.empty();
            _changeResourceNameModal.currentFileInfo.$me.prepend('[' + (isDir ? 'フォルダ' : 'ファイル') + '] ' + beforeName);

            _changeResourceNameModal.beforeName = beforeName;
            _changeResourceNameModal.isDir = isDir;

            _changeResourceNameModal.newName.$me.val(beforeName);

            this.$me.modal('show');
        } else {
            alert('名前を変更するファイルが存在しません。');
        }
    },
    hide: function() {
        this.$me.modal('hide');
    },
    btnOK: {
        $me: null,
        init: function() {
            this.$me = $('#btn_change_resource_name_submit');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                if (_changeResourceNameModal.form.isValid()) {
                    _loading.$me.fadeIn();
                    var selected = _ftpControlBlock.server.selectedServer;

                    if (_changeResourceNameModal.beforeName != '') {
                        var param = _changeResourceNameModal.form.getSerializeArray();
                        param.push({ name: 'env', value: _ftpControlBlock.server.selectedServer });
                        param.push({ name: 'isDir', value: _changeResourceNameModal.isDir });
                        param.push({ name: 'beforeName', value: _changeResourceNameModal.beforeName });

                        _ajax.call('POST', '/BGFTP/ChangeResourceName', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                _ftpControlBlock.server.reset(selected);
                            }
                        }, true);
                        _changeResourceNameModal.hide();

                        _ftpControlBlock.operationLogBlock.reset();
                    } else {
                        alert('名前を変更するファイルを選択してください！');
                        _changeResourceNameModal.hide();
                    }
                    _loading.$me.fadeOut();
                }

                return false;
            });
        }
    }
}

var _ftpControlBlock = {
    $me: null,
    init: function() {
        this.$me = $('.ftp-control');
        this.btnBackward.init();
        this.btnRefresh.init();
        this.btnResourceRemove.init();
        this.btnChangeResourceName.init();
        this.btnMakeDirectory.init();
        this.btnDownload.init();
        this.btnUpload.init();
        this.server.init();
        this.form.init();
        this.operationLogBlock.init();
        this.hostMenuBlock.init();
    },
    form: {
        $me: null,
        init: function() {
            this.$me = $('#form_ftp_control');
        },
        getSerializeArray: function() {
            return this.$me.serializeArray();
        },
        isValid: function() {
            return this.$me.validate().form();
        }
    },
    hostMenuBlock: {
        $me: null,
        init: function() {
            this.$me = $('.host-control');
            this.hostList.init();
            this.btnHostOption.init();
            this.btnHostConnect.init();
            this.btnHostDisconnect.init();
        },
        reset: function() {
            $.get('/BGHost/ViewHostList', function(data) {
                _ftpControlBlock.hostMenuBlock.$me.replaceWith(data);
                _ftpControlBlock.hostMenuBlock.init();
            });
        },
        hostList: {
            $me: null,
            init: function() {
                this.$me = $('select[name="host_list"]');
            }
        },
        btnHostOption: {
            $me: null,
            init: function() {
                this.$me = $('#btn_host_option');
                this.click();
            },
            click: function() {
                this.$me.click(function() {
                    _hostOptionModal.show();
                });
            }
        },
        btnHostConnect: {
            $me: null,
            init: function() {
                this.$me = $('#btn_host_connect');
                this.click();
            },
            click: function() {
                this.$me.click(function() {
                    _ftpControlBlock.hostMenuBlock.hostList.$me.find('option:selected').each(function() {
                        var param = _connectionConfig.form.getSerializeArray();
                        param.push({ name: 'selectHostId', value: $(this).val() });
                        _ajax.call('POST', '/BGFTP/Connect', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                window.location.href = '/FTP/';
                            }
                        }, true);
                    });
                });
            }
        },
        btnHostDisconnect: {
            $me: null,
            init: function() {
                this.$me = $('#btn_host_disconnect');
                this.click();
            },
            click: function() {
                this.$me.click(function() {
                    var param = _ftpControlBlock.form.getSerializeArray();
                    _ajax.call('POST', '/BGFTP/Disconnect', param, function(response) {
                        if (response != null && response.Result == 'success') {
                            window.location.href = '/';
                        }
                    }, true);
                });
            }
        }
    },
    btnInit:function(){
        if (!_ftpControlBlock.server.local.isSelecting && !_ftpControlBlock.server.remote.isSelecting) {
            _ftpControlBlock.btnActive(false);
        } else {
            _ftpControlBlock.btnActive(true);
        }
    },
    btnActive:function(active){
        _ftpControlBlock.btnResourceRemove.active(active);
        _ftpControlBlock.btnChangeResourceName.active(active);
        _ftpControlBlock.btnMakeDirectory.active(active);
    },
    btnBackward: {
        $me: null,
        init: function() {
            this.$me = $('#btn_backward');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                if (_ftpControlBlock.server.selectedServer != null) {
                    _loading.$me.fadeIn();
                    $.get('/BGFTP/ViewServerInfo?path=..&env=' + _ftpControlBlock.server.selectedServer, function(data) {
                        if (_ftpControlBlock.server.selectedServer == 'local') {
                            _ftpControlBlock.server.local.$me.replaceWith(data);
                            _ftpControlBlock.server.local.init();
                            _ftpControlBlock.server.setSelectedServer('local');
                        } else {
                            _ftpControlBlock.server.remote.$me.replaceWith(data);
                            _ftpControlBlock.server.remote.init();
                            _ftpControlBlock.server.setSelectedServer('remote');
                        }
                    });
                    _loading.$me.fadeOut();
                }
            });
        }
    },
    btnRefresh: {
        $me: null,
        init: function() {
            this.$me = $('#btn_refresh');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                if (_ftpControlBlock.server.selectedServer != null) {
                    _loading.$me.fadeIn();
                    _ftpControlBlock.server.allReset();
                    _loading.$me.fadeOut();
                }
            });
        }
    },
    btnResourceRemove: {
        $me: null,
        init: function() {
            this.$me = $('.resource-remove');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                if (_ftpControlBlock.server.selectedServer != null) { 

                    var isRemove = false;
                    var selected = _ftpControlBlock.server.selectedServer;
                    var fileName = '';
                    var isDir = false;
                    var fileList = [];

                    if (selected == 'local') {
                        $.each(_ftpControlBlock.server.local.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                            fileName = $(element).attr('data-fileid');
                            if (fileName != '..') {
                                if ($(element).find('i').hasClass('icon-folder-1')) {
                                    isDir = true;
                                } else {
                                    isDir = false;
                                }
                                var data = [fileName, isDir];
                                fileList.push(data);
                                isRemove = true;
                            }
                        });
                    } else {
                        $.each(_ftpControlBlock.server.remote.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                            fileName = $(element).attr('data-fileid');
                            if (fileName != '..') {
                                if ($(element).find('i').hasClass('icon-folder-1')) {
                                    isDir = true;
                                } else {
                                    isDir = false;
                                }
                                var data = [fileName, isDir];
                                fileList.push(data);
                                isRemove = true;
                            }
                        });
                    }

                    if (isRemove && confirm('ファイルを削除しますか？')) {
                        var param = _ftpControlBlock.form.getSerializeArray();
                        param.push({ name: 'env', value: selected });
                        param.push({ name: 'resList', value: fileList.join(':') });

                        _ajax.call('POST', '/BGFTP/RemoveResource', param, function(response) {
                            if (response != null && response.Result == 'success') {
                                _ftpControlBlock.server.reset(selected);
                            }
                        }, true);

                        _ftpControlBlock.operationLogBlock.reset();
                    } else {
                        alert('削除するファイルが存在しません。');
                    }
                    _ftpControlBlock.server.local.contextMenu.hide();
                    _ftpControlBlock.server.remote.contextMenu.hide();
                    _ftpControlBlock.btnInit();
                }
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
    btnChangeResourceName: {
        $me: null,
        init: function() {
            this.$me = $('.change-resource-name');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                if (_ftpControlBlock.server.selectedServer != null) {
                    _changeResourceNameModal.show();
                }
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
    btnMakeDirectory: {
        $me: null,
        init: function() {
            this.$me = $('.make-directory');
            this.click();
            this.active(false);
        },
        click: function() {
            this.$me.click(function() {
                if (_ftpControlBlock.server.selectedServer != null) {
                    if (_makeDirectoryModal.directoryName.$me.hasClass('input-validation-error')) {
                        _makeDirectoryModal.directoryName.$me.removeClass('input-validation-error');
                        $('.field-validation-error span').remove();
                    }
                    _makeDirectoryModal.show();
                }
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
    btnDownload: {
        $me: null,
        init: function() {
            this.$me = $('.resource-download');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                _loading.$me.fadeIn();
                _ftpControlBlock.server.download();
                _loading.$me.fadeOut();
            });
        }
    },
    btnUpload: {
        $me: null,
        init: function() {
            this.$me = $('.resource-upload');
            this.click();
        },
        click: function() {
            this.$me.click(function() {
                _loading.$me.fadeIn();
                _ftpControlBlock.server.upload();
                _loading.$me.fadeOut();
            });
        }
    },
    server: {
        $me: null,
        selectedServer: 'local',
        init: function() {
            this.$me = $('.server');
            this.local.init();
            this.remote.init();
            _ftpControlBlock.server.setSelectedServer('local');
        },
        allReset: function() {
            $.get('/BGFTP/ViewServerInfo?path=*', function(data) {
                _ftpControlBlock.server.$me.replaceWith(data);
                _ftpControlBlock.server.local.isSelecting = false;
                _ftpControlBlock.server.remote.isSelecting = false;
                _ftpControlBlock.server.init();
                return false;
            });
        },
        reset: function(env) {
            $.get('/BGFTP/ViewServerInfo?path=*&env=' + env, function(data) {
                if (env == 'local') {
                    _ftpControlBlock.server.local.$me.replaceWith(data);
                    _ftpControlBlock.server.local.init();
                    _ftpControlBlock.server.local.isSelecting = false;
                    _ftpControlBlock.server.setSelectedServer('local');
                } else {
                    _ftpControlBlock.server.remote.$me.replaceWith(data);
                    _ftpControlBlock.server.remote.init();
                    _ftpControlBlock.server.remote.isSelecting = false;
                    _ftpControlBlock.server.setSelectedServer('remote');
                }

                _ftpControlBlock.btnInit();

                return false;
            });
        },
        setSelectedServer: function(env) {
            _ftpControlBlock.server.selectedServer = env;
            if (env == 'local') {
                _ftpControlBlock.server.local.$me.css('border', '2px solid #555');
                _ftpControlBlock.server.remote.$me.css('border', '2px solid #999');
            }
            else {
                _ftpControlBlock.server.remote.$me.css('border', '2px solid #555');
                _ftpControlBlock.server.local.$me.css('border', '2px solid #999');
            }
        },
        download: function() {
            var isTarget = false;

            $.each(_ftpControlBlock.server.remote.list.$me.find('li.ui-selected, li.ui-selecting'), function(index, element) {
                isTarget = true;
                return false;
            });

            if (isTarget) {
                var isDownload = false;
                var fileList = [];

                $.each(_ftpControlBlock.server.remote.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                    var fileName = '';
                    var isDir = false;
                    fileName = $(element).attr('data-fileid');
                    if (fileName != '..') {
                        if ($(element).find('i').hasClass('icon-folder-1')) {
                            isDir = true;
                        } else {
                            isDir = false;
                        }
                        var data = [fileName, isDir];
                        fileList.push(data);
                        isDownload = true;
                    }
                });

                if (isDownload) {
                    var param = _ftpControlBlock.form.getSerializeArray();
                    param.push({ name: 'resList', value: fileList.join(':') });

                    _ajax.call('POST', '/BGFTP/Download', param, function(response) {
                        if (response != null && response.Result == 'success') {
                            _ftpControlBlock.server.reset('local');
                        }
                    }, true);
                }

                _ftpControlBlock.operationLogBlock.reset();
                _ftpControlBlock.server.remote.list.$me.find('li').removeClass('ui-selected').removeClass('ui-selecting');
            } else {
                alert('ダウンロードするファイルを選択してください。');
            }
            _ftpControlBlock.server.remote.contextMenu.hide();
        },
        upload: function() {
            var isTarget = false;
            $.each(_ftpControlBlock.server.local.list.$me.find('li.ui-selected, li.ui-selecting'), function(index, element) {
                isTarget = true;
                return false;
            });

            if (isTarget) {
                var isUpload = false;
                var fileList = [];

                $.each(_ftpControlBlock.server.local.list.$me.find('li.ui-selecting, li.ui-selected'), function(index, element) {
                    var fileName = '';
                    var isDir = false;
                    fileName = $(element).attr('data-fileid');
                    if (fileName != '..') {
                        if ($(element).find('i').hasClass('icon-folder-1')) {
                            isDir = true;
                        } else {
                            isDir = false;
                        }
                        var data = [fileName, isDir];
                        fileList.push(data);
                        isUpload = true;
                    }
                });

                if (isUpload) {
                    var param = _ftpControlBlock.form.getSerializeArray();
                    param.push({ name: 'resList', value: fileList.join(':') });

                    _ajax.call('POST', '/BGFTP/Upload', param, function(response) {
                        if (response != null && response.Result == 'success') {
                            _ftpControlBlock.server.reset('remote');
                        }
                    }, true);
                }
                _ftpControlBlock.operationLogBlock.reset();
                _ftpControlBlock.server.local.list.$me.find('li').removeClass('ui-selected').removeClass('ui-selecting');
            } else {
                alert('アップロードするファイルを選択してください。');
            }
            _ftpControlBlock.server.local.contextMenu.hide();
        },
        local: {
            $me: null,
            isSelecting: false,
            init: function() {
                this.$me = $('.local-server');
                this.filterBlock.init();
                this.contextMenu.init();
                this.list.init();
            },
            filterBlock:{
                $me:null,
                init:function(){
                    this.$me = $('.local-filter-block');
                    this.pattern.init();
                    this.btnSubmit.init();
                    this.btnReset.init();
                },
                btnSubmit:{
                    $me: null,
                    init: function() {
                        this.$me = $('#btn_local_filter_submit');
                        this.click();
                    },
                    click: function() {
                        this.$me.click(function(e) {
                            _loading.$me.fadeIn();

                            $.get('/BGFTP/ViewServerInfo?path=*&env=local&filterPattern=' + _ftpControlBlock.server.local.filterBlock.pattern.$me.val(), function(data) {

                                _ftpControlBlock.server.local.$me.replaceWith(data);
                                _ftpControlBlock.server.local.init();
                                _ftpControlBlock.server.local.isSelecting = false;
                                _ftpControlBlock.server.setSelectedServer('local');
                                _ftpControlBlock.btnInit();
                            });
                            _loading.$me.fadeOut();
                        });
                    }
                },
                btnReset: {
                    $me: null,
                    init: function() {
                        this.$me = $('#btn_local_filter_reset');
                        this.click();
                    },
                    click: function() {
                        this.$me.click(function(e) {
                            _loading.$me.fadeIn();
                            $.get('/BGFTP/ViewServerInfo?path=*&env=local', function(data) {
                                _ftpControlBlock.server.local.$me.replaceWith(data);
                                _ftpControlBlock.server.local.init();
                                _ftpControlBlock.server.local.isSelecting = false;
                                _ftpControlBlock.server.setSelectedServer('remote');
                                _ftpControlBlock.btnInit();
                            });
                            _loading.$me.fadeOut();
                        });
                    }
                },
                pattern: {
                    $me: null,
                    init: function() {
                        this.$me = $('#LocalFilterPattern');
                        this.focus();
                    },
                    focus: function() {
                        this.$me.focus(function() {
                            _ftpControlBlock.server.setSelectedServer('local');
                        });
                    }
                }
            },
            contextMenu: {
                $me: null,
                init: function() {
                    this.$me = $('.context-menu-local');
                    this.upload.init();
                    this.remove.init();
                    this.changeName.init();
                },
                upload:{
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-upload');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                remove:{
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-local-remove');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                changeName: {
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-local-change-name');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                show: function(left, top) {

                    var isView = false;
                    $.each(_ftpControlBlock.server.local.list.$me.find('li.ui-selected, li.ui-selecting'), function(index, element) {
                        isView = true;
                    });

                    if (isView) {
                        _ftpControlBlock.server.local.contextMenu.upload.show();
                        _ftpControlBlock.server.local.contextMenu.remove.show();
                        _ftpControlBlock.server.local.contextMenu.changeName.show();
                    } else {
                        _ftpControlBlock.server.local.contextMenu.upload.hide();
                        _ftpControlBlock.server.local.contextMenu.remove.hide();
                        _ftpControlBlock.server.local.contextMenu.changeName.hide();
                    }


                    this.$me.css('display', 'block');
                    this.$me.css('left', left);
                    this.$me.css('top', top);
                },
                hide: function() {
                    this.$me.css('display', 'none');
                }
            },
            list: {
                $me: null,
                init: function() {
                    this.$me = $('.local-server-list');
                    this.fileList.init();
                    this.setUI();
                    this.click();
                    this.$me.bind('contextmenu', function() {
                        return false;
                    });
                },
                fileList: {
                    $me: null,
                    init: function() {
                        this.$me = $('.local-file-list');
                    }
                },
                setUI: function() {
                    this.setSelectable();
                    this.setDraggable();
                    this.setDroppable();
                },
                setSelectable: function() {
                    this.$me.selectable({
                        filter: 'li.local-file-item, li.remote-file-item',
                        selected: function(event, ui) {
                            _ftpControlBlock.server.setSelectedServer('local');
                            _ftpControlBlock.server.local.isSelecting = true;
                            _ftpControlBlock.btnInit();
                        },
                        unselected: function(event, ui) {
                            _ftpControlBlock.server.local.isSelecting = false;
                            _ftpControlBlock.btnInit();
                        }
                    });
                },
                setDraggable: function() {
                    this.$me.find('li.local-file-item, li.remote-file-item').draggable({
                        helper: function() {
                            var selected = _ftpControlBlock.server.local.list.$me.find('li.ui-selected, li.ui-selecting');
                            var container = $('<div/>');
                            container.append(selected.clone().removeClass('ui-selected').removeClass('ui-selecting'));

                            _ftpControlBlock.server.setSelectedServer('local');
                            _ftpControlBlock.server.local.isSelecting = true;
                            _ftpControlBlock.btnInit();

                            return container;
                        }
                    });
                },
                setDroppable: function() {
                    this.$me.droppable({
                        drop: function(ev, ui) {
                            if (!_hostOptionModal.isShow) {
                                _loading.$me.fadeIn();
                                _ftpControlBlock.server.download();
                                _loading.$me.fadeOut();
                            }
                        }
                    });
                },
                click: function() {
                    this.$me.find('li.local-file-item, li.remote-file-item').click(function(event) {
                        if (event.metaKey == false) {
                            _ftpControlBlock.server.local.list.$me.find('li').removeClass('ui-selected').removeClass('ui-selecting');
                            $(this).addClass('ui-selecting');
                        }
                        else {
                            if ($(this).hasClass('ui-selected')) {
                                $(this).removeClass('ui-selected');
                            }
                            else {
                                $(this).addClass('ui-selecting');
                            }
                        }
                        _ftpControlBlock.server.setSelectedServer('local');
                        _ftpControlBlock.server.local.isSelecting = true;
                        _ftpControlBlock.btnInit();
                    });



                    this.$me.bind('contextmenu', function(e) {
                        _ftpControlBlock.server.setSelectedServer('local');
                        _ftpControlBlock.server.remote.contextMenu.hide();
                        _ftpControlBlock.server.local.contextMenu.show(e.pageX, e.pageY);
                        _ftpControlBlock.btnInit();
                    });
                }
            }
        },
        remote: {
            $me: null,
            isSelecting: false,
            init: function() {
                this.$me = $('.remote-server');
                this.filterBlock.init();
                this.contextMenu.init();
                this.list.init();
            },
            filterBlock: {
                $me: null,
                init: function() {
                    this.$me = $('.remote-filter-block');
                    this.pattern.init();
                    this.btnSubmit.init();
                    this.btnReset.init();
                },
                btnSubmit: {
                    $me: null,
                    init: function() {
                        this.$me = $('#btn_remote_filter_submit');
                        this.click();
                    },
                    click: function() {
                        this.$me.click(function(e) {
                            _loading.$me.fadeIn();
                            $.get('/BGFTP/ViewServerInfo?path=*&env=remote&filterPattern=' + _ftpControlBlock.server.remote.filterBlock.pattern.$me.val(), function(data) {
                                _ftpControlBlock.server.remote.$me.replaceWith(data);
                                _ftpControlBlock.server.remote.init();
                                _ftpControlBlock.server.remote.isSelecting = false;
                                _ftpControlBlock.server.setSelectedServer('remote');
                                _ftpControlBlock.btnInit();
                            });
                            _loading.$me.fadeOut();
                        });
                    }
                },
                btnReset: {
                    $me: null,
                    init: function() {
                        this.$me = $('#btn_remote_filter_reset');
                        this.click();
                    },
                    click: function() {
                        this.$me.click(function(e) {
                            _loading.$me.fadeIn();
                            $.get('/BGFTP/ViewServerInfo?path=*&env=remote', function(data) {
                                _ftpControlBlock.server.remote.$me.replaceWith(data);
                                _ftpControlBlock.server.remote.init();
                                _ftpControlBlock.server.remote.isSelecting = false;
                                _ftpControlBlock.server.setSelectedServer('remote');
                                _ftpControlBlock.btnInit();
                            });
                            _loading.$me.fadeOut();
                        });
                    }
                },
                pattern: {
                    $me: null,
                    init: function() {
                        this.$me = $('#RemoteFilterPattern');
                        this.focus();
                    },
                    focus: function() {
                        this.$me.focus(function() {
                            _ftpControlBlock.server.setSelectedServer('remote');
                        });
                    }
                }
            },
            contextMenu: {
                $me: null,
                init: function() {
                    this.$me = $('.context-menu-remote');
                    this.download.init();
                    this.remove.init();
                    this.changeName.init();
                },
                download:{
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-download');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                remove:{
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-remote-remove');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                changeName: {
                    $me: null,
                    init: function() {
                        this.$me = $('.context-menu-remote-change-name');
                    },
                    show: function() {
                        this.$me.css('display', 'block');
                    },
                    hide: function() {
                        this.$me.css('display', 'none');
                    }
                },
                show: function(left, top) {
                    var isView = false;
                    $.each(_ftpControlBlock.server.remote.list.$me.find('li.ui-selected, li.ui-selecting'), function(index, element) {
                        isView = true;
                    });

                    if (isView) {
                        _ftpControlBlock.server.remote.contextMenu.download.show();
                        _ftpControlBlock.server.remote.contextMenu.remove.show();
                        _ftpControlBlock.server.remote.contextMenu.changeName.show();
                    } else {
                        _ftpControlBlock.server.remote.contextMenu.download.hide();
                        _ftpControlBlock.server.remote.contextMenu.remove.hide();
                        _ftpControlBlock.server.remote.contextMenu.changeName.hide();
                    }

                    this.$me.css('display', 'block');
                    this.$me.css('left', left);
                    this.$me.css('top', top);
                },
                hide: function() {
                    this.$me.css('display', 'none');
                }
            },
            list: {
                $me: null,
                init: function() {
                    this.$me = $('.remote-server-list');
                    this.fileList.init();
                    this.setUI();
                    this.click();
                    this.$me.bind('contextmenu', function() {
                        return false;
                    });
                },
                fileList: {
                    $me: null,
                    init: function() {
                        this.$me = $('.remote-file-list');
                    }
                },
                setUI: function() {
                    this.setSelectable();
                    this.setDraggable();
                    this.setDroppable();
                },
                setSelectable: function() {
                    this.$me.selectable({
                        filter: 'li.local-file-item, li.remote-file-item',
                        selected: function(event, ui) {
                            _ftpControlBlock.server.setSelectedServer('remote');
                            _ftpControlBlock.server.remote.isSelecting = true;
                            _ftpControlBlock.btnInit();
                        },
                        unselected: function(event, ui) {
                            _ftpControlBlock.server.remote.isSelecting = false;
                            _ftpControlBlock.btnInit();
                        }
                    });
                },
                setDraggable: function() {
                    this.$me.find('li.remote-file-item, li.local-file-item').draggable({
                        helper: function() {
                            var selected = _ftpControlBlock.server.remote.list.$me.find('li.ui-selected, li.ui-selecting');
                            var container = $('<div/>');
                            container.append(selected.clone().removeClass('ui-selected').removeClass('ui-selecting'));

                            _ftpControlBlock.server.setSelectedServer('remote');
                            _ftpControlBlock.server.remote.isSelecting = true;

                            return container;
                        },
                        revert: 'invaild',
                        cursor: 'move'
                    });
                },
                setDroppable: function() {
                    this.$me.droppable({
                        drop: function(ev, ui) {
                            if (!_hostOptionModal.isShow) {
                                _loading.$me.fadeIn();
                                _ftpControlBlock.server.upload();
                                _loading.$me.fadeOut();
                            }
                        }
                    });
                },
                click: function() {
                    this.$me.find('li.local-file-item, li.remote-file-item').click(function(event) {
                        if (event.metaKey == false) {
                            _ftpControlBlock.server.remote.list.$me.find('li').removeClass('ui-selected').removeClass('ui-selecting');
                            $(this).addClass('ui-selecting');
                        }
                        else {
                            if ($(this).hasClass('ui-selected')) {
                                $(this).removeClass('ui-selected');
                            }
                            else {
                                $(this).addClass('ui-selecting');
                            }
                        }
                        _ftpControlBlock.server.setSelectedServer('remote');
                        _ftpControlBlock.server.remote.isSelecting = true;
                        _ftpControlBlock.btnInit();
                    });

                    this.$me.bind('contextmenu', function(e) {
                        _ftpControlBlock.server.setSelectedServer('remote');
                        _ftpControlBlock.server.local.contextMenu.hide();
                        _ftpControlBlock.server.remote.contextMenu.show(e.pageX, e.pageY);
                    });
                }
            }
        }
    },
    operationLogBlock: {
        $me: null,
        init: function() {
            this.$me = $('.ftp-operation-log');
            this.btnDownloadLog.init();
            this.btnRemoveLog.init();
            this.log.init();
        },
        reset: function() {
            $.get('/BGFTP/ViewLog', function(data) {
                _ftpControlBlock.operationLogBlock.$me.replaceWith(data);
                _ftpControlBlock.operationLogBlock.init();
                _ftpControlBlock.operationLogBlock.log.scollTop();
            });
        },
        log: {
            $me: null,
            init: function() {
                this.$me = $('#OperationLog');
            },
            scollTop: function() {
                _ftpControlBlock.operationLogBlock.log.$me.animate({
                    scrollTop: _ftpControlBlock.operationLogBlock.log.$me[0].scrollHeight - _ftpControlBlock.operationLogBlock.log.$me.height()
                }, 400);
            }
        },
        btnDownloadLog: {
            $me: null,
            init: function() {
                this.$me = $('#btn_download_log');
                this.click();
            },
            click: function() {
                this.$me.click(function() {
                    _ajax.call('POST', '/BGFTP/DownloadLog', _ftpControlBlock.form.getSerializeArray(), function(response) {
                        if (response != null && response.Result == 'success') {
                            _ftpControlBlock.operationLogBlock.reset();
                        }
                    }, true);
                });
            }
        },
        btnRemoveLog: {
            $me: null,
            init: function() {
                this.$me = $('#btn_remove_log');
                this.click();
            },
            click: function() {
                this.$me.click(function() {
                    _ajax.call('POST', '/BGFTP/RemoveLog', _ftpControlBlock.form.getSerializeArray(), function(response) {
                        if (response != null && response.Result == 'success') {
                            _ftpControlBlock.operationLogBlock.reset();
                        }
                    }, true);
                });
            }
        }
    }
}

function setLocalServer(data) {
    _ftpControlBlock.server.local.$me.replaceWith(data.responseText);
    _ftpControlBlock.server.local.init();
    _ftpControlBlock.server.local.isSelecting = false;
    _ftpControlBlock.btnInit();
    _ftpControlBlock.server.setSelectedServer('local');
}

function setRemoteServer(data) {
    _ftpControlBlock.server.remote.$me.replaceWith(data.responseText);
    _ftpControlBlock.server.remote.init();
    _ftpControlBlock.server.remote.isSelecting = false;
    _ftpControlBlock.btnInit();
    _ftpControlBlock.server.setSelectedServer('remote');

}