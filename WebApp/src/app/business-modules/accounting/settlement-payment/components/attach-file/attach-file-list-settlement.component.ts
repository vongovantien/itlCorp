import { takeUntil } from 'rxjs/operators';
import { ActivatedRoute, Params } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppForm } from '@app';
import { AccountingRepo } from '@repositories';
import { SysImage } from '@models';
import { InjectViewContainerRefDirective } from '@directives';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'settlement-attach-file-list',
    templateUrl: './attach-file-list-settlement.component.html',
    styleUrls: ['./attach-file-list-settlement.component.scss'],
})

export class SettlementAttachFileListComponent extends AppForm implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) confirmPopupContainerRef: InjectViewContainerRefDirective;
    @Output() onChange: EventEmitter<SysImage[]> = new EventEmitter<SysImage[]>();

    _id: string;

    files: SysImage[] = [];
    selectedFile: SysImage;

    constructor(
        private _accountingRepo: AccountingRepo,
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
                    this._id = p.id;
                    this.getFiles(this._id);
                }
            );
    }

    getFiles(id: string) {
        this._accountingRepo.getAttachedFiles('Settlement', id)
            .subscribe(
                (data: any) => {
                    this.files = data || [];
                    this.onChange.emit(this.files);
                }
            )
    }

    chooseFile(_$event: any) {
        const fileList: FileList[] = event.target['files'];
        if (fileList.length > 0 && !!this._id) {
            this._accountingRepo.uploadAttachedFiles('Settlement', this._id, fileList)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.getFiles(this._id);
                        }
                    }
                );
        }
    }

    deleteFile(_file: SysImage) {
        this.selectedFile = _file;
        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.confirmPopupContainerRef.viewContainerRef, {
            body: 'Do you want to delete this file ?',
            labelConfirm: 'Yes',
            labelCancel: 'No'
        }, () => {
            this.onDeleteFile(this.selectedFile.id)
        })

    }

    onDeleteFile(id: string) {
        this._accountingRepo.deleteAttachedFile('Settlement', id)
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

}