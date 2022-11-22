import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HouseBill } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { getHBLSState, IShareBussinessState } from '../../../store';
import { DocumentationRepo } from './../../../../../shared/repositories/documentation.repo';

@Component({
    selector: 'app-mass-update-pod',
    templateUrl: './mass-update-pod.component.html',
})
export class ShareBussinessMassUpdatePodComponent extends PopupBase implements OnInit {

    @Input() jobId: string = '';
    @Output() isUpdated: EventEmitter<boolean> = new EventEmitter<boolean>();

    formGroup: FormGroup;
    deliveryDate: AbstractControl;
    deliveryPerson: AbstractControl;
    HAWBNo: AbstractControl;
    houseBillList: HouseBill[] = [];
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
    }

    getHouseBills() {
        this._store.select(getHBLSState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (hbls: any[]) => {
                    if (hbls?.length >= 1) {
                        this.houseBillList = [new HouseBill({ id: 'All', hwbno: 'All' })];
                        this.houseBillList = this.houseBillList.concat(hbls) || [];
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

    updatePOD() {
        this.isSubmitted = true;
        const deliveryDateInput = !!this.deliveryDate.value?.startDate ? formatDate(this.deliveryDate.value?.startDate, 'yyyy-MM-dd', 'en') : null;
        if (this.formGroup.invalid || deliveryDateInput === null) {
            return;
        }
        const listHbl = !!this.HAWBNo.value && this.HAWBNo.value[0].id === 'All' ?
            this.houseBillList.filter(x => x.id !== 'All').map((x: any) => x.id) :
            this.HAWBNo.value.map((x: any) => x.id)
        const body = {
            deliveryDate: deliveryDateInput,
            deliveryPerson: this.deliveryPerson.value,
            houseBills: listHbl,
        }
        this._documentRepo.updateMultipleProofOfDelivery(body).pipe(catchError(this.catchError)).subscribe(
            (res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toast.success(res.message);
                    this.isUpdated.emit(true);
                    this.isSubmitted = false;
                    this.formGroup.reset();
                    this.hide();
                } else {
                    this._toast.error(res.message);
                }
            }

        )
    }

    resetDeliveryDate() {
        this.deliveryDate.setValue(null);
    }

}
