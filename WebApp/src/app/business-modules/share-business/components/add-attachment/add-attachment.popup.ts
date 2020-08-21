import { PopupBase } from "src/app/popup.base";
import { OnInit, Component, Output, EventEmitter } from "@angular/core";
import { DocumentationRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { NgProgress } from "@ngx-progressbar/core";
import { IAppState } from "@store";
import { Store } from "@ngrx/store";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { Params, ActivatedRoute } from "@angular/router";


@Component({
    selector: "add-attachment-popup",
    templateUrl: "./add-attachment.popup.html",
})
export class ShareBusinessAddAttachmentPopupComponent extends PopupBase implements OnInit {
    checkAll: boolean = false;
    jobId: string;
    files: IShipmentAttachFile[] = [];
    @Output() onAdd: EventEmitter<any> = new EventEmitter<any>();

    constructor(
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _activedRoute: ActivatedRoute,
        private _store: Store<IAppState>
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
            { title: 'Attach File', field: 'name' }
        ];
    }

    getFileShipment(jobId: string) {
        this.isLoading = true;
        this._documentRepo.getShipmentFilesAttachPreAlert(jobId).
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
            this._documentRepo.uploadFileShipment(this.jobId, true, fileList)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
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
        this.onAdd.emit(lstFiles);
        this.hide();
    }

    onCancel() {
        this.hide();
    }

    removeAllChecked() {
        this.checkAll = false;
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
