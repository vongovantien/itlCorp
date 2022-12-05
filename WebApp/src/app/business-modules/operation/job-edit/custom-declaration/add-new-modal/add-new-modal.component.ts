import { AfterViewInit, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { OpsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { OperationRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { PopupBase } from 'src/app/popup.base';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { CustomClearanceFormDetailComponent } from '../../../custom-clearance/components/form-detail-clearance/form-detail-clearance.component';

@Component({
    selector: 'add-new-modal',
    templateUrl: './add-new-modal.component.html'
})
export class AddNewModalComponent extends PopupBase implements OnInit, AfterViewInit {
    @ViewChild(CustomClearanceFormDetailComponent) detailComponent: CustomClearanceFormDetailComponent;
    @Output() isCloseModal = new EventEmitter();
    @Input() currentJob: OpsTransaction;

    customDeclaration: CustomClearance;

    constructor(
        private _operationRepo: OperationRepo,
        private _store: Store<IAppState>,
        private cdRef: ChangeDetectorRef,
        private _toastr: ToastrService,
    ) {
        super();
    }

    ngAfterViewInit(): void {
        this._store.select(getOperationTransationState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.setFormValue(res.opstransaction)
                    }
                }
            );

        this.cdRef.detectChanges();
    }

    setFormValue(data: OpsTransaction) {
        this.detailComponent.customerName = data.customerName;
        this.detailComponent.formGroup.patchValue({
            partnerTaxCode: data.customerAccountNo,
            gateway: data.placeNameCode,
            serviceType: data.productService,
            hblid: data.hwbno,
            type: data.serviceMode,
            grossWeight: data.sumGrossWeight,
            netWeight: data.sumNetWeight,
            cbm: data.sumCbm,
            shipper: data.shipper,
            consignee: data.consignee,
        })
    }


    createCustomerDeclaration() {
        this.detailComponent.getClearance();
        this.detailComponent.isSubmitted = true;
        this.detailComponent.isConvertJob = false;

        if (!this.detailComponent.isDisableCargo && !this.detailComponent.cargoType.value) {
            return;
        }
        if (this.detailComponent.formGroup.invalid || (!!this.detailComponent.clearanceDate.value && !this.detailComponent.clearanceDate.value.startDate)) {
            return;
        }
        this.detailComponent.customDeclaration.jobNo = this.currentJob.jobNo;

        this._operationRepo.addCustomDeclaration(this.detailComponent.customDeclaration)
            .subscribe((res: CommonInterface.IResult) => {
                if (!!res && res.status) {
                    this._toastr.success(res.message)
                    this.detailComponent.formGroup.reset();
                    this.detailComponent.isSubmitted = false;
                    this.isCloseModal.emit(true)
                }
            });
    }

    closePopUpAddNew() {
        this.detailComponent.formGroup.reset();
        this.isCloseModal.emit(true)
    }

}
