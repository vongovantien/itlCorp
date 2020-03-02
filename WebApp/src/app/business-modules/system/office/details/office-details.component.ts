import { Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Department, Office } from '@models';

import { IFormAddOffice, OfficeFormAddComponent } from '../components/form-add-office/form-add-office.component';
import { AppPage } from 'src/app/app.base';
import { SystemRepo } from '@repositories';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-office-details',
    templateUrl: './office-details.component.html'
})
export class OfficeDetailsComponent extends AppPage {
    @ViewChild(OfficeFormAddComponent, { static: false }) formAdd: OfficeFormAddComponent;
    formData: IFormAddOffice = {
        id: '',
        branchNameVn: '',
        branchNameEn: '',
        bankAccountName_VN: '',
        bankAccountName_EN: '',

        buid: '',
        addressVn: '',
        addressEn: '',
        tel: '',
        fax: '',
        email: '',
        taxcode: '',
        bankAccountVND: '',
        bankAccountUSD: '',
        bankAddress_Local: '',
        code: '',
        swiftCode: '',
        shortName: '',
        userCreated: '',
        datetimeCreated: '',
        userModified: '',
        datetimeModified: '',
        active: true,
        company: '',
        bankAddress_En: '',
        location: ''
    };
    officeId: string = '';

    office: Office;
    departments: Department[] = [];

    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _activedRouter: ActivatedRoute,
        private _router: Router,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.officeId = param.id;
                this.getDetailOffice(this.officeId);
            } else {
                this._router.navigate(["home/system/office"]);
            }
        });

    }

    updateOffice() {
        this.formAdd.isSubmited = true;
        if (this.formAdd.formGroup.valid) {
            this._progressRef.start();
            const body: any = {
                id: this.officeId,
                code: this.formAdd.code.value,
                branchNameEn: this.formAdd.branchNameEn.value,
                branchNameVn: this.formAdd.branchNameVn.value,
                shortName: this.formAdd.shortName.value,
                addressEn: this.formAdd.addressEn.value,
                buid: this.formAdd.company.value,
                addressVn: this.formAdd.addressVn.value,
                taxcode: this.formAdd.taxcode.value,
                tel: this.formAdd.tel.value,
                fax: this.formAdd.fax.value,
                email: this.formAdd.email.value,
                bankAccountVnd: this.formAdd.bankAccountVND.value,
                bankAccountUsd: this.formAdd.bankAccountUSD.value,
                bankAccountNameVn: this.formAdd.bankAccountName_VN.value,
                bankAccountNameEn: this.formAdd.bankAccountName_EN.value,
                bankAddressLocal: this.formAdd.bankAddress_Local.value,
                bankAddressEn: this.formAdd.bankAddress_En.value,
                active: this.formAdd.active.value,
                swiftCode: this.formAdd.swiftCode.value,
                location: this.formAdd.location.value

            };
            this._systemRepo.updateOffice(body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.getDetailOffice(this.officeId);

                        } else {
                            this._toastService.warning(res.message);
                        }
                    }
                );
        }
    }

    getDetailOffice(id: string) {
        this._progressRef.start();
        this._systemRepo.getDetailOffice(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                tap(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.office = new Office(res.data);
                            this.formAdd.isDetail = true;
                            this.formData.id = res.data.id;
                            this.formData.code = res.data.code;
                            this.formData.branchNameEn = res.data.branchNameEn;
                            this.formData.branchNameVn = res.data.branchNameVn;
                            this.formData.shortName = res.data.shortName;
                            this.formData.addressEn = res.data.addressEn;
                            this.formData.taxcode = res.data.taxcode;
                            this.formData.tel = res.data.tel;
                            this.formData.email = res.data.email;
                            this.formData.fax = res.data.fax;
                            this.formData.buid = res.data.buid;
                            this.formData.swiftCode = res.data.swiftCode;
                            this.formData.bankAddress_Local = res.data.bankAddressLocal;
                            this.formData.bankAddress_En = res.data.bankAddressEn;
                            this.formData.addressVn = res.data.addressVn;
                            this.formData.bankAccountVND = res.data.bankAccountVnd;
                            this.formData.bankAccountUSD = res.data.bankAccountUsd;
                            this.formData.bankAccountName_VN = res.data.bankAccountNameVn;
                            this.formData.bankAccountName_EN = res.data.bankAccountNameEn;
                            this.formData.location = res.data.location;
                            this.formAdd.SelectedOffice = new Office(res.data);
                            this.formData.company = res.data.buid;
                            this.formData.active = res.data.active;
                            setTimeout(() => {
                                this.formAdd.update(this.formData, res.data.active);
                            }, 300);
                        }
                    }
                ),
                switchMap(() => this._systemRepo.getDepartmentsByOfficeId(id))
            )
            .subscribe(
                (res: any) => {
                    this.departments = res;
                    this.formAdd.getDepartment(this.departments);
                    console.log(this.departments);
                },
            );
    }
}
