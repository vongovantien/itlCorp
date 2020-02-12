import { Component, Input } from '@angular/core';
import { Company, Office, PermissionSample } from '@models';
import { SystemRepo } from '@repositories';
import { finalize, catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';


@Component({
    selector: 'add-role-user',
    templateUrl: 'add-role-user.component.html'
})

export class AddRoleUserComponent extends AppList {
    @Input() userId: string = '';

    companies: Company[] = [];
    listRoles: PermissionSample[] = [];
    offices: Office[] = [];
    roles: any[] = [];
    selectedIdOffice: string = '';
    officeData: Office[] = [];
    constructor(
        private _systemRepo: SystemRepo,

    ) {
        super();
    }
    ngOnInit() {
        this.headers = [
            { title: 'Role Name', field: 'name', },
            { title: 'Company', field: '' },
            { title: 'Office', field: 'officeId' },
        ];
        this.getDataCombobox();
        this.getCompanies();
        this.getOffices();
        this.getPermissionsByUserId();
    }

    getDataCombobox() {
        this._systemRepo.getComboboxPermission()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.roles = res;
                },
            );
    }

    getCompanies() {
        this._systemRepo.getListCompany()
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.companies = res;
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
                    this.officeData = res;
                    console.log(this.offices);
                },
            );
    }

    getPermissionsByUserId() {
        this._systemRepo.getListPermissionsByUserId(this.userId).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                this.listRoles = res;
                console.log(this.listRoles);
            },
        );
    }

    selectedCompany(id: string) {
        const obj = this.officeData.filter(x => x.buid === id);
        this.offices = obj;
        this.listRoles.forEach(element => {

            element.buid = id;


        });
    }


    addNewLine() {
        this.listRoles.push(new PermissionSample());
        console.log(this.listRoles);

    }




}