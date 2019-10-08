import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { CompanyInformationFormAddComponent } from '../components/form-add-company/form-add-company.component';
import { SystemRepo } from 'src/app/shared/repositories';
import { Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-add-company-info',
    templateUrl: './add-company-infomation.component.html',
})
export class CompanyInformationAddComponent extends AppPage {

    @ViewChild(CompanyInformationFormAddComponent, { static: false }) formAdd: CompanyInformationFormAddComponent;

    constructor(
        private _systemRepo: SystemRepo,
        private _router: Router,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();

    }

    ngOnInit(): void { }

    cancel() {
        this._router.navigate(["home/system/company"]);
    }

    create() {
        this._progressRef.start();
        const body: ICompanyAdd = {
            companyCode: this.formAdd.code.value,
            companyNameEn: this.formAdd.bunameEn.value,
            companyNameVn: this.formAdd.bunameVn.value,
            companyNameAbbr: this.formAdd.bunameAbbr.value,
            website: this.formAdd.website.value,
            photoUrl: 'https://picsum.photos/id/457/400/400', // TODO lấy url hình
            status: this.formAdd.inactive.value.value,
            photoName: 'https://picsum.photos/id/457/400/400',
        };
        this._systemRepo.addNewCompany(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, '');
                    } else {
                        this._toastService.error(res.message, '');
                    }
                }
            );
    }
}

interface ICompanyAdd {
    companyCode: string;
    companyNameEn: string;
    companyNameVn: string;
    companyNameAbbr: string;
    website: string;
    photoName: string;
    photoUrl: string;
    status: boolean;
}
