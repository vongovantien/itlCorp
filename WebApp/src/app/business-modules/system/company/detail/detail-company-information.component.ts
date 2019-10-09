import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';
import { IFormAddCompany, CompanyInformationFormAddComponent } from '../components/form-add-company/form-add-company.component';
import { Company } from 'src/app/shared/models';

@Component({
    selector: 'app-detail-company-info',
    templateUrl: './detail-company-information.component.html',
    styleUrls: ['./detail-company-information.component.scss']
})
export class CompanyInformationDetailComponent extends AppPage {
    @ViewChild(CompanyInformationFormAddComponent, { static: false }) formAddCompany: CompanyInformationFormAddComponent;
    formData: IFormAddCompany = {
        code: 'sss',
        bunameEn: '',
        bunameVn: '',
        bunameAbbr: '',
        website: '',
        inactive: false
    };
    companyId: string = '';

    company: Company;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _router: Router,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
    ) {
        super();
        this._progressRef = this._progressService.ref();


    }

    ngOnInit(): void {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.companyId = param.id;
                this.getDetailCompany(this.companyId);
            } else {
                this._router.navigate(["home/system/company"]);
            }
        });

    }

    getDetailCompany(id: string) {
        this._progressRef.start();
        this._systemRepo.getDetailCompany(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.company = new Company(res.data);

                        this.formData.code = res.data.code;
                        this.formData.bunameAbbr = res.data.bunameAbbr;
                        this.formData.bunameEn = res.data.bunameEn;
                        this.formData.bunameVn = res.data.bunameVn;
                        this.formData.website = res.data.website;
                        this.formData.inactive = res.data.inactive;

                        this.formAddCompany.photoUrl = this.company.logoPath;
                        this.formAddCompany.formGroup.patchValue(this.formData);
                        this.formAddCompany.inactive.setValue(this.formAddCompany.types.filter(i => i.value === res.data.inactive)[0]);
                        console.log(res.data);
                    }
                }
            );
    }

    viewForm() {
        console.log(this.formData);
    }
}




