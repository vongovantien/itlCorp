import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ShareBusinessAddAttachmentPopupComponent } from '../add-attachment/add-attachment.popup';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { IAppState, getParamsRouterState } from '@store';
import { Params } from '@angular/router';

@Component({
    selector: 'share-pre-alert',
    templateUrl: './pre-alert.component.html'
})
export class ShareBusinessReAlertComponent extends AppList {
    @ViewChild(ShareBusinessAddAttachmentPopupComponent, { static: false }) attachmentPopup: ShareBusinessAddAttachmentPopupComponent;
    files: IShipmentAttachFile[] = [];
    jobId: string;


    constructor(
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _store: Store<IAppState>

    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

    }
    ngOnInit(): void {
        this._store.select(getParamsRouterState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((params: Params) => {
                if (params.jobId) {
                    this.jobId = params.jobId;
                }
            });
        this.headers = [
            { title: 'Attach File', field: 'name' }
        ];
    }

    onAddFile(files: any) {
        if (this.files.length === 0) {
            this.files = files;
        } else {
            files.forEach(element => {
                for (const file of this.files) {
                    if (file.id !== element.id) {
                        this.files.push(element);
                        break;
                    }
                }
            });
        }

        const filesToUpdate = files.filter(f => f.isTemp === true);
        if (filesToUpdate.length > 0) {
            this._progressRef.start();
            this._documentRepo.updateFilesToShipment(filesToUpdate)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res) {
                            this._toastService.success("Upload file successfully!");

                        }
                    }
                );
        }
    }

    showPopup() {
        this.attachmentPopup.files.forEach(element => {
            element.isChecked = false;

        });
        this.attachmentPopup.checkAll = false;
        this.attachmentPopup.getFileShipment(this.jobId);
        this.attachmentPopup.show();


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
}
