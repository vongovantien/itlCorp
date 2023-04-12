import { formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild, ViewEncapsulation } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { IShareBussinessState, getHBLSState, getTransactionDetailCsTransactionType } from '@share-bussiness';
import moment from 'moment';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { ShareDocumentTypeAttachComponent } from '../../edoc/document-type-attach/document-type-attach.component';
import { GetListHBLSuccessAction } from './../../../store/actions/hbl.action';

@Component({
    selector: 'app-mass-update-pod',
    templateUrl: './mass-update-pod.component.html',
    styleUrls: ['./mass-update-pod.scss'],
    encapsulation: ViewEncapsulation.None
})
export class ShareBussinessMassUpdatePodComponent extends PopupBase implements OnInit {

    @Input() jobId: string = '';
    @Output() isUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();
    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;
    formGroup: FormGroup;
    deliveryDate: AbstractControl;
    deliveryPerson: AbstractControl;
    HAWBNo: AbstractControl;
    houseBillList: any[] = [];
    deliveryDateAll: AbstractControl;
    deliveryPersonAll: string = null;
    headersAttachFile: CommonInterface.IHeaderTable[];
    transactionType: string = "";

    constructor(
        private _fb: FormBuilder,
        private _toast: ToastrService,
        private _progressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        protected _store: Store<IShareBussinessState>,
        private _cd: ChangeDetectorRef
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    proofOfDelivery: ProofOfDelivery = new ProofOfDelivery();

    ngOnInit() {
        this.headers = [
            { title: 'HAWB No', field: 'hawbNo', sortable: true },
            { title: 'Delivery Person', field: 'deliveryPerson', sortable: true },
            { title: 'Delivery Date', field: 'deliveryDate', sortable: true },
        ]
        this.headersAttachFile = [
            { title: 'Alias Name', field: 'aliasName', width: 200 },
            { title: 'Real File Name', field: 'realFilename', width: 250 },
            { title: 'Document Type', field: 'docType', required: true },
            { title: 'House Bill No', field: 'hbl', width: 250 },
            { title: 'Note', field: 'note' },
        ]
        this.getHouseBillList();
        this.initForm();
        this._store.select(getTransactionDetailCsTransactionType)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: string) => this.transactionType = res)
    }

    initForm() {
        this.formGroup = this._fb.group({
            deliveryPerson: [null, Validators.compose([
                Validators.required
            ])],
            deliveryDate: [null, Validators.compose([
                Validators.required
            ])],
            HAWBNo: [null, Validators.required],
            deliveryDateAll: [],
        });
        this.deliveryPerson = this.formGroup.controls['deliveryPerson'];
        this.deliveryDate = this.formGroup.controls['deliveryDate'];
        this.HAWBNo = this.formGroup.controls['HAWBNo'];
        this.deliveryDateAll = this.formGroup.controls['deliveryDateAll'];
    }

    resetDeliveryDate(index: number) {
        this.houseBillList[index].deliveryDate = null;
    }

    onShowDocumentAttach() {
        this.documentAttach.headers = this.headersAttachFile;
        this.documentAttach.getDocumentType();
        this.documentAttach.show();
    }

    onChangeAllValuePOD() {
        this.houseBillList.map(x => {
            x.deliveryDate = !!this.deliveryDateAll.value && !!this.deliveryDateAll.value.startDate ? this.deliveryDateAll.value : null,
                x.deliveryPerson = this.deliveryPersonAll
        })
    }

    updatePOD() {
        this.isSubmitted = true;
        const body = this.houseBillList.map(x =>
        ({
            deliveryDate: (!!x.deliveryDate && !!x.deliveryDate.startDate) ? formatDate(x.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : null,
            deliveryPerson: x.deliveryPerson,
            hblId: x.id
        }))
        this._documentRepo.updateMultipleProofOfDelivery(body)
            .pipe(catchError(this.catchError)).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res?.status) {
                        this._toast.success(res.message);
                        const payload = this.houseBillList.map(x => ({
                            ...x,
                            deliveryDate: (!!x.deliveryDate && !!x.deliveryDate.startDate) ?
                                formatDate(x.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : null,
                        }));
                        this._store.dispatch(new GetListHBLSuccessAction(payload));
                    } else {
                        this._toast.error(res.message);
                    }
                    this.onClosePopUp();
                }
            )
    }

    getHouseBillList(): void {
        this._store.select(getHBLSState).subscribe((res: any[]) => {
            this.houseBillList = this.mapValueToForm(res)
        });
    }

    mapValueToForm(houBillList: any[]): any[] {
        return houBillList.map(x => ({
            ...x,
            deliveryDate: x.deliveryDate && { startDate: moment(x.deliveryDate), endDate: moment(x.deliveryDate) }
        }));
    }


    onClosePopUp() {
        this.isSubmitted = false;
        this.formGroup.reset();
        this.deliveryDateAll.setValue(null);
        this.deliveryPersonAll = null;
        this.getHouseBillList();
        this.hide();
    }

    resetFormControl(control: FormControl | AbstractControl) {
        if (!!control && control instanceof FormControl) {
            control.setValue(null);
            control.markAsUntouched({ onlySelf: true });
            control.markAsPristine({ onlySelf: true });
        }
    }
}
