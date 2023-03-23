import { formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HouseBill } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { IShareBussinessState, getHBLSState } from '@share-bussiness';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
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
    houseBillList: any[] = [];
    deliveryDateAll: AbstractControl;
    deliveryPersonAll: string = null;
    headersAttachFile: CommonInterface.IHeaderTable[];
    documentType: any = [{
        nameEn: "abc",
        id: "3"
    }]
    houseBillListTemp: any[] = [];
    constructor(
        private _fb: FormBuilder,
        private _toast: ToastrService,
        private _progressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _cd: ChangeDetectorRef,
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
            { title: 'Alias Name', field: 'aliasName', width: 200 },
            { title: 'Real File Name', field: 'realFilename', width: 250 },
            { title: 'Document Type', field: 'docType', required: true },
            { title: 'House Bill No', field: 'hbl', width: 250 },
            { title: 'Note', field: 'note' },
        ]
        //this.getHouseBills();
        this.initForm()
    }

    // ngAfterViewInit(): void {
    //     this.getHouseBills();
    // }

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
        this.houseBillListTemp.map(x => {
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
        this._store.select(getHBLSState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any[]) => {
                    if (res?.length >= 1) {
                        this.houseBillList = res;
                        this.houseBillListTemp = res;
                        console.log(res)
                        console.log(this.houseBillListTemp)
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
                        this.onClosePopUp();
                    } else {
                        this._toast.error(res.message);
                    }
                }
            )
    }

    onClosePopUp() {
        this._cd.detectChanges();
        this.isSubmitted = false;
        this.formGroup.reset();
        this.deliveryDateAll.setValue(null);
        this.deliveryPersonAll = null;
        this.houseBillListTemp = this.houseBillList;
        this.hide();
    }
}
