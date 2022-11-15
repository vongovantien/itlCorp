import { PopupBase } from "src/app/popup.base";
import { OnInit, Component, Output, EventEmitter, ViewChild, ViewChildren, QueryList } from "@angular/core";
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { NgProgress } from "@ngx-progressbar/core";
import { IAppState } from "@store";
import { Store } from "@ngrx/store";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { Params, ActivatedRoute } from "@angular/router";
import { ConfirmPopupComponent } from "@common";
import { ContextMenuDirective } from "@directives";


@Component({
    selector: "add-attachment-popup",
    templateUrl: "./add-attachment.popup.html",
    styleUrls: ['./../../../accounting/components/attach-file/attach-file-list.component.scss']
})
export class ShareBusinessAddAttachmentPopupComponent extends PopupBase implements OnInit {
    checkAll: boolean = false;
    jobId: string;
    files: IShipmentAttachFile[] = [];
    @Output() onAdd: EventEmitter<any> = new EventEmitter<any>();
    selectedFile: IShipmentAttachFile;
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;

    constructor(
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _activedRoute: ActivatedRoute,
        private _store: Store<IAppState>,
        private _systemFileManagerRepo: SystemFileManageRepo,
        private _exportRepo: ExportRepo
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

    }

    ngOnInit(): void {
        this._activedRoute.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((params: Params) => {
                if (params.jobId) {
                    this.jobId = params.jobId;
                    this.getFileShipment(this.jobId);
                }
            });
        this.headers = [
            { title: 'File Name', field: 'name' },
            { title: 'Attach Person', field: 'userCreated' }
        ];
    }

    getFileShipment(jobId: string) {
        this.isLoading = true;
        this._systemFileManagerRepo.getFile('Document', 'Shipment', jobId).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: IShipmentAttachFile[] = []) => {
                    this.files = res;
                    this.files.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                }
            );
    }

    chooseFile(event: any) {
        const fileList: FileList[] = event.target['files'];
        if (fileList.length > 0) {
            this._progressRef.start();
            this._systemFileManagerRepo.uploadAttachedFileEdoc('Document', 'Shipment', this.jobId, fileList)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            if (!!this.jobId) {
                                this.getFileShipment(this.jobId);
                            }
                        }
                    }
                );
        }
        event.target.value = '';
    }

    checkAllChange() {
        if (this.checkAll) {
            this.files.forEach(x => {
                x.isChecked = true;
            });
        } else {
            this.files.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    onAddToMail() {
        const lstFiles = this.files.filter(x => x.isChecked === true);
        if (lstFiles.length === 0) {
            return;
        }
        console.log(lstFiles);

        this.onAdd.emit(lstFiles);
        this.hide();
    }

    onCancel() {
        this.hide();
    }

    removeAllChecked() {
        this.checkAll = false;
    }

    deleteFile(file: IShipmentAttachFile) {
        if (!!file) {
            this.selectedFile = file;
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'Are you sure to delete this file ?',
                labelCancel: 'No',
                labelConfirm: 'Yes'
            }, () => { this.onDeleteFile(); });
        }
    }

    getFileShipmentUrls() {
        return this.files;
    }

    onDeleteFile() {
        this._systemFileManagerRepo.deleteFile('Document', 'Shipment', this.jobId, this.selectedFile.name)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success("File deleted successfully!");
                        this.getFileShipment(this.jobId);
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

    selectFileItem(file: IShipmentAttachFile) {
        this.selectedFile = file;
        this.selectedFile.isChecked = !this.selectedFile.isChecked;
    }
    onSelectFileMenuContext(file: IShipmentAttachFile) {
        this.selectedFile = file;
        this.clearMenuContext(this.queryListMenuContext);
    }

    viewFile() {
        const extension = this.selectedFile.url.split('.').pop();
        if (['xlsx'].includes(extension)) {
            this._exportRepo.previewExport(this.selectedFile.url);
        } else {
            this._exportRepo.downloadExport(this.selectedFile.url);
        }
    }

    download() {
        this._exportRepo.downloadExport(this.selectedFile.url);
    }

    selectFile() {
        this.onAdd.emit([this.selectedFile]);
        console.log(this.selectedFile);
        this.hide();
    }
}
interface IShipmentAttachFile {
    id: string;
    name: string;
    thumb: string;
    url: string;
    folder: string;
    objectId: string;
    extension: string;
    userCreated: string;
    dateTimeCreated: string;
    fileName: string;
    isChecked: boolean;
}
