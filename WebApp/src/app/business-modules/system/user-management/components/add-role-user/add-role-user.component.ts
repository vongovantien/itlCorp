import { Component, OnInit } from '@angular/core';
import { Company, Office } from '@models';
import { SystemRepo } from '@repositories';
import { finalize, catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';


@Component({
    selector: 'add-role-user',
    templateUrl: 'add-role-user.component.html'
})

export class AddRoleUserComponent extends AppList {
    companies: Company[] = [];
    offices: Office[] = [];
    listRoles: any[] = [];

    roles: CommonInterface.ICommonTitleValue[] = [
        { title: 'Customer Service', value: 'Customer Service' },
        { title: 'Field OPS', value: 'Field OPS' },
        { title: 'Accountant', value: 'Accountant' },
        { title: 'Admin', value: 'Admin' },
    ];

    constructor(
        private _systemRepo: SystemRepo,

    ) {
        super();
    }
    ngOnInit() {

        this.getCompanies();
        this.getOffices();
    }

    getCompanies() {
        this._systemRepo.getListCompany()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.companies = res;
                    this.companies = this.companies.filter(x => x.active === true);
                    console.log(this.companies);
                },
            );
    }

    getOffices() {
        this._systemRepo.getListOffices()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.offices = res;
                    this.offices = this.offices.filter(x => x.active === true);
                    console.log(this.offices);
                },
            );
    }

}