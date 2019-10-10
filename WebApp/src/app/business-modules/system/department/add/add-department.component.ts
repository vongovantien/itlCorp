import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AccountingRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { BaseService } from 'src/app/shared/services';
import { Department } from 'src/app/shared/models/system/department';

@Component({
    selector: 'app-department-new',
    templateUrl: './add-department.component.html'
})

export class DepartmentAddNewComponent extends AppPage {
    formAdd: FormGroup;
    departmentCode: AbstractControl;
    nameEn: AbstractControl;
    nameLocal: AbstractControl;
    nameAbbr: AbstractControl;
    office: AbstractControl;
    status: AbstractControl;
    statusList: CommonInterface.ICommonTitleValue[] = [];
    officeList: CommonInterface.ICommonTitleValue[] = [];

    isValidForm: boolean = false;
    isSubmited: boolean = false;

    constructor(
        //private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private cdRef: ChangeDetectorRef,
        private _fb: FormBuilder,
        private _baseService: BaseService
    ) {
        super();

        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initDataInform();
        this.initForm();
    }

    initForm() {
        this.formAdd = this._fb.group({
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
            status: [this.statusList[0]]
        });

        this.departmentCode = this.formAdd.controls['departmentCode'];
        this.nameEn = this.formAdd.controls['nameEn'];
        this.nameLocal = this.formAdd.controls['nameLocal'];
        this.nameAbbr = this.formAdd.controls['nameAbbr'];
        this.office = this.formAdd.controls['office'];
        this.status = this.formAdd.controls['status'];
    }

    initDataInform() {
        this.statusList = this.getStatus();
    }

    ngAfterViewInit() {
        //this.requestSurchargeListComponent.isShowButtonCopyCharge = true;
        //this.cdRef.detectChanges(); // * Force to update view
    }

    saveDepartment() {
        this.isSubmited = true;
        if (this.formAdd.valid) {
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

    getStatus(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false }
        ];
    }

    back() {
        this._router.navigate(['home/system/department']);
    }

}


