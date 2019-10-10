import { Component, OnInit, ViewChild } from '@angular/core';
import { IFormAddOffice, OfficeFormAddComponent } from '../components/form-add-office/form-add-office.component';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { AppPage } from 'src/app/app.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { Office } from 'src/app/shared/models/system/office';
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
        bankAccountName: '',
        buid: '',
        addressVn: '',
        addressEn: '',
        tel: '',
        fax: '',
        email: '',
        taxcode: '',
        bankAccountVND: '',
        bankAccountUSD: '',
        bankName: '',
        bankAddress: '',
        code: '',
        swiftCode: '',
        shortName: '',
        active: true
    };
    officeId: string = '';

    office: Office;


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
        this._progressRef.start();
        const body: any = {
            id: this.officeId,
            code: this.formAdd.code.value,
            branchNameEn: this.formAdd.branchNameEn.value,
            branchNameVn: this.formAdd.branchNameVn.value,
            shortName: this.formAdd.shortName.value,
            addressEn: this.formAdd.addressEn.value,
            buid: this.formAdd.selectedCompany.value,
            addressVn: this.formAdd.addressVn.value,
            taxcode: this.formAdd.taxcode.value,
            tel: this.formAdd.tel.value,
            fax: this.formAdd.fax.value,
            email: this.formAdd.email.value,
            bankAccountVnd: this.formAdd.bankAccountVND.value,
            bankName: this.formAdd.bankName.value,
            bankAccountName: this.formAdd.bankAccountName.value,
            active: this.formAdd.active.value.value,
            bankAddress: this.formAdd.bankAddress.value,
            swiftCode: this.formAdd.swiftCode.value

        };
        this._systemRepo.updateOffice(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.warning(res.message);

                    }
                }
            );
    }

    getDetailOffice(id: string) {
        this._progressRef.start();
        this._systemRepo.getDetailOffice(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.office = new Office(res.data);
                        console.log(this.office);

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
                        this.formData.bankAddress = res.data.bankAddress;
                        this.formData.bankName = res.data.bankName;
                        this.formData.addressVn = res.data.addressVn;
                        this.formData.bankAccountVND = res.data.bankAccountVnd;
                        this.formData.bankAccountUSD = res.data.bankAccountUsd;
                        this.formData.bankAccountName = res.data.bankAccountName;
                        // this.formAdd.selectedCompany = { field: res.data.sysCompany.bunameEn, value: res.data.sysCompany.id };
                        console.log(this.formAdd.selectedCompany);

                        setTimeout(() => {
                            this.formAdd.selectedCompany = { field: 'id', value: res.data.sysCompany.id };
                            // this.formAdd.active.setValue(this.formAdd.status.filter(i => i.value === res.data.active)[0]);

                            this.formAdd.update(this.formData, res.data.active);
                        }, 300);

                        // this.formAdd.company.setValue(res.data.buid);


                        console.log(res.data);

                    }
                },
                (errors: any) => { },
                () => {
                    this._progressRef.complete();
                }
            );

    }



}
