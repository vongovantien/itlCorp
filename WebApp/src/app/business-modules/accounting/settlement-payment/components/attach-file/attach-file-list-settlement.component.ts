import { takeUntil } from 'rxjs/operators';
import { ISettlementPaymentState } from '../store/reducers/index';
import { ActivatedRoute, Params } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Component, OnInit, ViewChild, Output, EventEmitter, Input, ChangeDetectionStrategy } from '@angular/core';
import { AppForm } from '@app';
import { AccountingRepo } from '@repositories';
import { SysImage } from '@models';
import { InjectViewContainerRefDirective } from '@directives';
import { ConfirmPopupComponent } from '@common';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Store } from '@ngrx/store';
import { ISettlementPaymentData } from '../../detail/detail-settlement-payment.component';
@Component({
    selector: 'settlement-attach-file-list',
    templateUrl: './attach-file-list-settlement.component.html',
    styleUrls: ['./attach-file-list-settlement.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SettlementAttachFileListComponent extends AppForm implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) confirmPopupContainerRef: InjectViewContainerRefDirective;
    @Output() onChange: EventEmitter<SysImage[]> = new EventEmitter<SysImage[]>();
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    //////
    get readonly(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;
    private _id: string;

    files: SysImage[] = [];
    selectedFile: SysImage;
    settlementPayment: ISettlementPaymentData;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _store: Store<ISettlementPaymentState>
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
        this.loadData();
    }

    loadData() {
        this._store.select('settlement-payment').subscribe((data: ISettlementPaymentState) => this.settlementPayment = data.detail.settlement);
    }
    getFiles(id: string) {
        this._accountingRepo.getAttachedFiles('Settlement', id)
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
        if (this._readonly) {
            return;
        }
        this.selectedFile = _file;
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

    dowloadAllAttach() {
        if (this.settlementPayment) {
            var arr = this.settlementPayment.settlement.settlementNo.split("/");
            var model = {
                folderName: "Settlement",
                folderId: this._id,
                fileName: arr[0] + "_" + arr[1] + ".zip"
            }
            this._accountingRepo.dowloadallAttach(model)
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, "application/zip", model.fileName);
                    }
                )
        }
    }
    filterViewFile() {
        if (this.files) {
            var type = ["xlsx","xls", "doc", "docx","pdf","txt"];
            for (let i = 0; i < this.files.length; i++) {
                var f = this.files[i];
                if (type.includes(f.name.split(".")[1])) {
                    f.dowFile = true 
                    f.viewFileUrl = `https://view.officeapps.live.com/op/view.aspx?src=${f.url}`;
                }
                else{
                    f.dowFile = false;
                    f.viewFileUrl = f.url;
                }
            }
        }
    }
}