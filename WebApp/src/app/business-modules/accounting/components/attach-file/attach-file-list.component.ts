import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppForm } from '@app';
import { ConfirmPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { SysImage } from '@models';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';

@Component({
    selector: 'accounting-attach-file-list',
    templateUrl: './attach-file-list.component.html',
    styleUrls: ['./attach-file-list.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class AccoutingAttachFileListComponent extends AppForm implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) confirmPopupContainerRef: InjectViewContainerRefDirective;
    @Output() onChange: EventEmitter<SysImage[]> = new EventEmitter<SysImage[]>();
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    @Input() fileNo?: String;
    @Input() folderModuleName?: string;
    @Input() chillId?: string;
    @Input() objId?: string;
    //////
    get readonly(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;
    private _id: string;

    files: SysImage[] = [];
    selectedFile: SysImage;

    @Input() accType: string = '';
    @Input() accId: string = '';

    constructor(
        private _fileRepo: SystemFileManageRepo,
        private _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
    ) {
        super();
    }

    ngOnInit() {
        this._activedRoute.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (p: Params) => {
                    this._id = p.id ? p.id : this.objId;
                    this.getFiles(this._id);
                }
            );
    }

    getFiles(id: string) {
        this._fileRepo.getAttachedFiles(this.folderModuleName, id)
            .subscribe(
                (data: any) => {
                    this.files = data || [];
                    this.filterViewFile();
                    this.onChange.emit(this.files);
                }
            )
    }

    chooseFile(_$event: any) {
        if (this._readonly) {
            return;
        }
        const fileList = event.target['files'];
        if (fileList.length > 0 && !!this._id) {
            let validSize: boolean = true;

            for (let i = 0; i <= fileList.length - 1; i++) {
                const fileSize: number = fileList[i].size / Math.pow(1024, 2); //TODO Verify BE
                if (fileSize >= 100) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toastService.warning("maximum file size < 100Mb");
                return;
            }
            this._fileRepo.uploadAttachedFileEdoc('Accounting', this.accType, this.accId, fileList)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.getFiles(this.accId);
                        }
                    }
                );
        }
    }

    deleteFile(_file: SysImage) {
        if (this._readonly) {
            return;
        }
        this.selectedFile = _file;
        console.log(this.selectedFile);

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.confirmPopupContainerRef.viewContainerRef, {
            body: 'Do you want to delete this file ?',
            labelConfirm: 'Yes',
            labelCancel: 'No',
            iconConfirm: 'la la-trash'
        }, () => {
            this.onDeleteFile(this.selectedFile.id)
        })

    }

    onDeleteFile(id: string) {
        this._fileRepo.deleteEdoc(id)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.getFiles(this._id);
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            )
    }

    dowloadAllAttach() {
        if (this.fileNo) {
            let fileName = "";

            if (this.fileNo.includes("/")) {
                let arr = this.fileNo.split("/");
                fileName = arr[0] + "_" + arr[1] + ".zip";
            } else {
                fileName = this.fileNo + ".zip";
            }

            let model = {
                folderName: this.folderModuleName,
                objectId: this._id,
                chillId: this.chillId,
                fileName: fileName
            }
            this._fileRepo.dowloadallAttach(model)
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, "application/zip", model.fileName);
                    }
                )
        }
    }
    filterViewFile() {
        if (this.files) {
            let type = ["xlsx", "xls", "doc", "docx"];
            for (let i = 0; i < this.files.length; i++) {
                let f = this.files[i];
                if (type.includes(f.name.split('.').pop())) {
                    f.dowFile = true
                    f.viewFileUrl = `https://gbc-excel.officeapps.live.com/op/view.aspx?src=${f.url}`;
                }
                else {
                    f.dowFile = false;
                    f.viewFileUrl = f.url;
                }
            }
        }
    }
}