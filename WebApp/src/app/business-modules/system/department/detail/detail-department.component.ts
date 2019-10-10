import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
//import { SettlementListChargeComponent } from '../components/list-charge-settlement/list-charge-settlement.component';
//import { SettlementFormCreateComponent } from '../components/form-create-settlement/form-create-settlement.component';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Department } from 'src/app/shared/models/system/department';

@Component({
    selector: 'app-department-detail',
    templateUrl: './detail-department.component.html'
})

export class DepartmentDetailComponent extends AppPage {

    //@ViewChild(SettlementListChargeComponent, { static: false }) requestSurchargeListComponent: SettlementListChargeComponent;
    //@ViewChild(SettlementFormCreateComponent, { static: false }) formCreateSurcharge: SettlementFormCreateComponent;
    formDetail: FormGroup;
    departmentCode: AbstractControl;
    nameEn: AbstractControl;
    nameLocal: AbstractControl;
    nameAbbr: AbstractControl;
    office: AbstractControl;
    company: AbstractControl;
    status: AbstractControl;
    statusList: CommonInterface.ICommonTitleValue[] = [];
    officeList: CommonInterface.ICommonTitleValue[] = [];

    isValidForm: boolean = false;
    isSubmited: boolean = false;
    constructor(
        private _activedRouter: ActivatedRoute,
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _fb: FormBuilder,
        private _progressService: NgProgress
    ) {
        super();

        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this.initDataInform();
        this.initForm();
    }

    initForm() {
        this.formDetail = this._fb.group({
            departmentCode: ['',
                Validators.compose([
                    Validators.required,
                    Validators.maxLength(50)
                ])
            ],
            nameEn: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            nameLocal: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            nameAbbr: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            office: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            company: [],
            status: [this.statusList[0]]
        });

        this.departmentCode = this.formDetail.controls['departmentCode'];
        this.nameEn = this.formDetail.controls['nameEn'];
        this.nameLocal = this.formDetail.controls['nameLocal'];
        this.nameAbbr = this.formDetail.controls['nameAbbr'];
        this.office = this.formDetail.controls['office'];
        this.company = this.formDetail.controls['company'];
        this.status = this.formDetail.controls['status'];
    }

    initDataInform() {
        this.statusList = this.getStatus();
    }

    updateDepartment() {
        this.isSubmited = true;
        if (this.formDetail.valid) {
            const dept: Department = {
                id: 0,
                code: this.departmentCode.value,
                deptName: this.nameLocal.value,
                deptNameEn: this.nameEn.value,
                deptNameAbbr: this.nameAbbr.value,
                officeName: this.office.value.value,
                company: '',
                status: this.status.value.value,
                managerId: '',
                userCreated: '',
                datetimeCreated: '',
                userModified: '',
                datetimeModified: '',
                active: true,
                inactiveOn: ''
            };
            console.log(dept);
        }

        // if (!this.requestSurchargeListComponent.surcharges.length) {
        //     this._toastService.warning(`Settlement payment don't have any surcharge in this period, Please check it again! `, '');
        //     return;
        // }

        // this._progressRef.start();
        // const body: IDataSettlement = {
        //     settlement: {
        //         id: "00000000-0000-0000-0000-000000000000",
        //         settlementNo: this.formCreateSurcharge.settlementNo.value,
        //         requester: this.formCreateSurcharge.requester.value,
        //         requestDate: formatDate(this.formCreateSurcharge.requestDate.value.startDate || new Date(), 'yyyy-MM-dd', 'en'),
        //         paymentMethod: this.formCreateSurcharge.paymentMethod.value.value,
        //         settlementCurrency: this.formCreateSurcharge.currency.value.id,
        //         note: this.formCreateSurcharge.note.value,
        //     },
        //     shipmentCharge: this.requestSurchargeListComponent.surcharges || []
        // };

        // this._accountingRepo.addNewSettlement(body)
        //     .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
        //     .subscribe(
        //         (res: CommonInterface.IResult) => {
        //             if (res.status) {
        //                 this._toastService.success(res.message);

        //                 this._router.navigate([`home/accounting/settlement-payment/${res.data.settlement.id}`]);
        //             } else {
        //                 this._toastService.warning(res.message);
        //             }
        //             this.requestSurchargeListComponent.selectedIndexSurcharge = null;
        //         }
        //     );
    }

    getDetailDeparment() {
        // this._progressRef.start();
        // this._accoutingRepo.getDetailSettlementPayment(settlementId)
        //     .pipe(
        //         catchError(this.catchError),
        //         finalize(() => this._progressRef.complete())
        //     )
        //     .subscribe(
        //         (res: ISettlementPaymentData) => {
        //             if (!res.settlement) {
        //                 this.back();
        //                 this._toastService.warning("Settlement not found");
        //                 return;
        //             }
        //             this.settlementPayment = res;

        //             switch (this.settlementPayment.settlement.statusApproval) {
        //                 case 'New':
        //                 case 'Denied':
        //                     break;
        //                 default:
        //                     this.formCreateSurcharge.form.disable();
        //                     this.formCreateSurcharge.isDisabled = true;

        //                     this.requestSurchargeListComponent.STATE = 'READ';
        //                     break;
        //             }

        //             // * wait to currecy list api
        //             setTimeout(() => {
        //                 this.formCreateSurcharge.form.setValue({
        //                     settlementNo: this.settlementPayment.settlement.settlementNo,
        //                     requester: this.settlementPayment.settlement.requester,
        //                     requestDate: new Date(this.settlementPayment.settlement.requestDate),
        //                     paymentMethod: this.formCreateSurcharge.methods.filter(method => method.value === this.settlementPayment.settlement.paymentMethod)[0],
        //                     note: this.settlementPayment.settlement.note,
        //                     amount: this.settlementPayment.chargeGrpSettlement.reduce((acc, curr) => acc + curr.totalAmount, 0),
        //                     currency: this.formCreateSurcharge.currencyList.filter(currency => currency.id === this.settlementPayment.settlement.settlementCurrency)[0]
        //                 });
        //             }, 300);

        //             this.requestSurchargeListComponent.surcharges = this.settlementPayment.chargeNoGrpSettlement;
        //             this.requestSurchargeListComponent.groupShipments = this.settlementPayment.chargeGrpSettlement;

        //             this.requestSurchargeListComponent.settlementCode = this.settlementPayment.settlement.settlementNo;

        //             // *SWITCH UI TO GROUP LIST SHIPMENT
        //             this.requestSurchargeListComponent.TYPE = typeCharge; // ? GROUP/LIST
        //             this.requestSurchargeListComponent.STATE = 'WRITE'; //  ? READ/WRITE
        //             this.requestSurchargeListComponent.isShowButtonCopyCharge = false;

        //             if (this.requestSurchargeListComponent.groupShipments.length) {
        //                 this.requestSurchargeListComponent.openAllCharge.next(true);
        //             }
        //         },
        //     );
    }


    back() {
        this._router.navigate(['home/system/department']);
    }

    getStatus(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false }
        ];
    }
}


