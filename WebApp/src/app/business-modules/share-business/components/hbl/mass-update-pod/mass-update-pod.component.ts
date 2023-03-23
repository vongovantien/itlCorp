import { formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { IShareBussinessState } from '@share-bussiness';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { ShareDocumentTypeAttachComponent } from '../../edoc/document-type-attach/document-type-attach.component';

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
    houseBillList: any[] = [];
    deliveryDateAll: AbstractControl;
    deliveryPersonAll: string = null;
    headersAttachFile: CommonInterface.IHeaderTable[];
    documentType: any = [{
        nameEn: "abc",
        id: "3"
    }]
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
    proofOfDelievey: ProofOfDelivery = new ProofOfDelivery();

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
        this.initForm()
        this.getHouseBills();
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

    onChangeAllValuePOD() {
        this.houseBillList.map(x => {
            x.deliveryDate = !!this.deliveryDateAll?.value?.startDate ? this.deliveryDateAll.value : null,
                x.deliveryPerson = this.deliveryPersonAll
        })
    }

    resetDeliveryDate() {
        this.deliveryDateAll?.setValue(null);
    }

    onShowDocumentAttach() {
        this.documentAttach.headers = this.headersAttachFile;
        this.documentAttach.show();
    }

    getHouseBills() {
        this._documentRepo.getHBLOfJob({ jobId: this.jobId })
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any[]) => {
                    if (res?.length >= 1) {
                        this.houseBillList = res;
                        console.log(res)
                    }
                }
            );
    }

    updatePOD() {
        this.isSubmitted = true;
        const body = this.houseBillList.map(x =>
        ({
            deliveryDate: !!x.deliveryDate?.startDate ? formatDate(x.deliveryDate?.startDate, 'yyyy-MM-dd', 'en') : null,
            deliveryPerson: x.deliveryPerson,
            hblId: x.id
        }))
        this._documentRepo.updateMultipleProofOfDelivery(body)
            .pipe(catchError(this.catchError)).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res?.status) {
                        this._toast.success(res.message);
                        this.getHouseBills();
                        this.isUpdated.emit(true);
                    } else {
                        this._toast.error(res.message);
                    }
                    this.onClosePopUp();
                }
            )
    }

    onClosePopUp() {
        this.isSubmitted = false;
        this.formGroup.reset();
        this.deliveryDateAll.setValue(null);
        this.deliveryPersonAll = null;
        console.log(this.houseBillList)
        this._cd.detectChanges();
        this.hide();
    }
}
