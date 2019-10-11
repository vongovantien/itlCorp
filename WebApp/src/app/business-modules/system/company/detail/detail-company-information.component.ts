import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { SystemRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, tap, switchMap, takeUntil } from 'rxjs/operators';
import { IFormAddCompany, CompanyInformationFormAddComponent } from '../components/form-add-company/form-add-company.component';
import { Company, Office } from 'src/app/shared/models';
import { ToastrService } from 'ngx-toastr';
import { AppForm } from 'src/app/app.form';
import { forkJoin } from 'rxjs';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-detail-company-info',
    templateUrl: './detail-company-information.component.html',
    styleUrls: ['./detail-company-information.component.scss']
})
export class CompanyInformationDetailComponent extends AppList {
    @ViewChild(CompanyInformationFormAddComponent, { static: false }) formAddCompany: CompanyInformationFormAddComponent;
    formData: IFormAddCompany = {
        code: 'sss',
        bunameEn: '',
        bunameVn: '',
        bunameAbbr: '',
        website: '',
        active: true
    };
    companyId: string = '';
    company: Company;

    offices: any[];

    headersOffice: CommonInterface.IHeaderTable[];

    constructor(
        private _activedRouter: ActivatedRoute,
        private _router: Router,
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _toastService: ToastrService
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.headersOffice = [
            { title: 'Office Code', field: 'code', sortable: true },
            { title: 'Name En', field: 'branchNameEn', sortable: true },
            { title: 'Name Local', field: 'branchNameVn', sortable: true },
            { title: 'Name Abbr', field: 'shortName', sortable: true },
            { title: 'Address En', field: 'addressEn', sortable: true },
            { title: 'Company', field: 'code', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];

    }

    ngOnInit(): void {
        this._activedRouter.params.pipe(
            catchError(this.catchError),
            takeUntil(this.ngUnsubscribe)
        ).subscribe((param: Params) => {
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
        forkJoin([
            this._systemRepo.getDetailCompany(id),
            this._systemRepo.getOfficeByCompany(id)
        ]).pipe(catchError(this.catchError))
            .subscribe(
                ([dataCompany, offices]) => {
                    if (dataCompany.status) {
                        this.getDataDetail(dataCompany.data);
                        this.getOffices(offices.data);
                    }
                }
            );
    }

    getDataDetail(company: Company) {
        this.company = new Company(company);
        this.formData.code = company.code;
        this.formData.bunameAbbr = company.bunameAbbr;
        this.formData.bunameEn = company.bunameEn;
        this.formData.bunameVn = company.bunameVn;
        this.formData.website = company.website;
        this.formData.active = company.active;
        this.formAddCompany.photoUrl = this.company.logoPath;

        this.formAddCompany.formGroup.patchValue(this.formData);
        this.formAddCompany.active.setValue(this.formAddCompany.types.filter(i => i.value === company.active)[0]);
        this.formAddCompany.code.disable();
    }

    getOffices(office: Office[]) {
        this.offices = (office || []).map((o: Office) => new Office(o));
        console.log(this.offices);
    }

    saveInformation() {
        this.formAddCompany.isSubmitted = true;
        if (this.formAddCompany.formGroup.invalid) {
            return;
        } else {
            const modelUpdeteCompany: any = {
                id: this.companyId,
                companyCode: this.formAddCompany.code.value,
                companyNameEn: this.formAddCompany.bunameEn.value,
                companyNameVn: this.formAddCompany.bunameEn.value,
                companyNameAbbr: this.formAddCompany.bunameEn.value,
                website: this.formAddCompany.bunameEn.value,
                photoName: this.formAddCompany.bunameEn.value,
                photoUrl: this.formAddCompany.photoUrl,
                status: this.formAddCompany.active.value.value,
            };

            this._progressRef.start();
            this._systemRepo.updateCompany(this.companyId, modelUpdeteCompany)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, 'Save Successfully');
                        } else {
                            this._toastService.warning(res.message);
                        }
                    }
                );
        }
    }

    cancel() {
        this._router.navigate(["home/system/company"]);
    }
}




