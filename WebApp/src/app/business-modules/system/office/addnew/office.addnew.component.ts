import { Component, ViewChild } from '@angular/core';
import { OfficeFormAddComponent } from '../components/form-add-office/form-add-office.component';
import { AppPage } from 'src/app/app.base';
import { NgProgress } from '@ngx-progressbar/core';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';



@Component({
    selector: 'app-office.addnew',
    templateUrl: './office.addnew.component.html'
})
export class OfficeAddNewComponent extends AppPage {
    @ViewChild(OfficeFormAddComponent, { static: false }) formAdd: OfficeFormAddComponent;
    constructor(private _progressService: NgProgress,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,
        private _router: Router
    ) {
        super();
        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
    }

    create() {
        this.formAdd.isSubmited = true;
        if (this.formAdd.formGroup.valid) {
            this._progressRef.start();
            const body: IOfficeAdd = {
                branchNameEn: this.formAdd.branchNameEn.value,
                branchNameVn: this.formAdd.branchNameVn.value,
                bankAccountName: this.formAdd.bankAccountName.value,
                buid: this.formAdd.selectedCompany.value,
                addressVn: this.formAdd.addressVn.value,
                addressEn: this.formAdd.addressEn.value,
                tel: this.formAdd.tel.value,
                fax: this.formAdd.fax.value,
                email: this.formAdd.email.value,
                taxcode: this.formAdd.taxcode.value,
                bankAccountVnd: this.formAdd.bankAccountVND.value,
                bankAccountUsd: '',
                bankName: this.formAdd.bankName.value,
                bankAddress: this.formAdd.bankAddress.value,
                code: this.formAdd.code.value,
                swiftCode: this.formAdd.swiftCode.value,
                shortName: this.formAdd.shortName.value,
                active: this.formAdd.active.value.value

            };
            this._systemRepo.addNewOffice(body)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                            this._router.navigate([`home/system/office`]);

                        } else {
                            this._toastService.error(res.message, '');
                        }
                    }
                );
        }


    }
}
interface IOfficeAdd {
    branchNameVn: string;
    branchNameEn: string;
    bankAccountName: string;
    buid: string;
    addressVn: string;
    addressEn: string;
    tel: string;
    fax: string;
    email: string;
    taxcode: string;
    bankAccountVnd: string;
    bankAccountUsd: string;
    bankName: string;
    bankAddress: string;
    code: string;
    swiftCode: string;
    shortName: string;
    active: boolean;
}
