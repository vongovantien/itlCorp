import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HouseBill } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { getHBLSState, IShareBussinessState } from '@share-bussiness';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { ShareDocumentTypeAttachComponent } from '../../document-type-attach/document-type-attach.component';

@Component({
    selector: 'app-mass-update-pod',
    templateUrl: './mass-update-pod.component.html',
    styleUrls: ['./mass-update-pod.scss']
})
export class ShareBussinessMassUpdatePodComponent extends PopupBase implements OnInit {

    @Input() jobId: string = '';
    @Output() isUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();
    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;
    formGroup: FormGroup;
    deliveryDate: AbstractControl;
    deliveryPerson: AbstractControl;
    HAWBNo: AbstractControl;
    houseBillList: HouseBill[] = [];
    deliveryDateAll: AbstractControl;
    deliveryPersonAll: string = null;
    headersAttachFile: CommonInterface.IHeaderTable[];
    documentType: any = {
        code: "abc",
        id: "3"
    }
    constructor(
        private _fb: FormBuilder,
        private _toast: ToastrService,
        private _progressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        protected _store: Store<IShareBussinessState>,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }
    proofOfDelievey: ProofOfDelivery = new ProofOfDelivery();

    ngOnInit() {
        this.headers = [
            { title: 'HAWB No', field: 'hawbNo', sortable: true },
            { title: 'Delivery Person', field: 'deliveryPerson', sortable: true },
            { title: 'Delivery Date', field: 'deliveryDate', sortable: true },
        ]
        this.headersAttachFile = [
            { title: 'No', field: 'index', width: 300 },
            { title: 'Alias Name', field: 'aliasName', width: 300 },
            { title: 'Real File Name', field: 'realFilename', width: 300 },
            { title: 'Document Type', field: 'docType', required: true },
            { title: 'House Bill No', field: 'hbl' },
            { title: 'Note', field: 'note' },
        ]
        this.getHouseBills();
        this.formGroup = this._fb.group({
            deliveryPerson: [null, Validators.compose([
                Validators.required
            ])],
            deliveryDate: [null, Validators.compose([
                Validators.required
            ])],
            HAWBNo: [null, Validators.required],
        }
        );
        this.deliveryPerson = this.formGroup.controls['deliveryPerson'];
        this.deliveryDate = this.formGroup.controls['deliveryDate'];
        this.HAWBNo = this.formGroup.controls['HAWBNo'];
        this.deliveryDateAll = this.formGroup.controls['deliveryDateAll'];
    }

    getHouseBills() {
        this._store.select(getHBLSState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbls: any[]) => {
                    if (hbls?.length >= 1) {
                        this.houseBillList = hbls;
                    }
                }
            );
    }

    selectedHAWBNo($event: any) {
        console.log($event);

        if ($event.length > 0) {
            if ($event[$event.length - 1].id === 'All') {
                this.HAWBNo.setValue([{ id: 'All', hwbno: 'All' }]);
            }
            else {
                const arrNotIncludeAll = $event.filter(x => x.id !== 'All'); //
                this.HAWBNo.setValue(arrNotIncludeAll);
            }
        }
    }

    onChangeAllValuePOD() {
        this.houseBillList.map(x => {
            x.deliveryDate = !!this.deliveryDate.value?.startDate ? formatDate(this.deliveryDate.value?.startDate, 'yyyy-MM-dd', 'en') : null,
                x.deliveryPerson = this.deliveryPerson.value
        })
    }
    // updatePOD() {
    //     this.isSubmitted = true;
    //     const deliveryDateInput = !!this.deliveryDate.value?.startDate ? formatDate(this.deliveryDate.value?.startDate, 'yyyy-MM-dd', 'en') : null;
    //     if (this.formGroup.invalid || deliveryDateInput === null) {
    //         return;
    //     }
    //     const listHbl = !!this.HAWBNo.value && this.HAWBNo.value[0].id === 'All' ?
    //         this.houseBillList.filter(x => x.id !== 'All').map((x: any) => x.id) :
    //         this.HAWBNo.value.map((x: any) => x.id)
    //     const body = {
    //         deliveryDate: deliveryDateInput,
    //         deliveryPerson: this.deliveryPerson.value,
    //         houseBills: listHbl,
    //     }
    //     this._documentRepo.updateMultipleProofOfDelivery(body).pipe(catchError(this.catchError)).subscribe(
    //         (res: CommonInterface.IResult) => {
    //             if (res.status) {
    //                 this._toast.success(res.message);
    //                 this.isUpdated.emit(true);
    //                 this.isSubmitted = false;
    //                 this.formGroup.reset();
    //                 this.hide();
    //             } else {
    //                 this._toast.error(res.message);
    //             }
    //         }

    //     )
    // }

    resetDeliveryDate() {
        this.deliveryDate.setValue(null);
    }

    onShowDocumentAttach() {
        this.documentAttach.headers = this.headersAttachFile;
        this.documentAttach.show();
    }
}
