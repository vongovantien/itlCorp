import { Component } from "@angular/core";
import { AppList } from "src/app/app.list";
import { User } from "@models";
import { SystemRepo } from "@repositories";
import { catchError } from "rxjs/operators";
import { UserLevel } from "src/app/shared/models/system/userlevel";

@Component({
    selector: 'add-user',
    templateUrl: 'add-user.component.html'
})

export class ShareSystemAddUserComponent extends AppList {

    constructor(
        private _systemRepo: SystemRepo
    ) {
        super();
    }
    users: any[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    positions: CommonInterface.INg2Select[];
    usersLevels: UserLevel[] = [];

    ngOnInit() {
        this.positions = [
            { id: 'Manager-Leader', text: 'Manager-Leader' },
            { id: 'Deputy', text: 'Deputy' },
            { id: 'Assistant', text: 'Assistant' },
        ];
        this.headers = [
            { title: 'User Name', field: 'username', },
            { title: 'Full Name', field: 'employeeNameVn' },
            { title: 'Position', field: 'Position' },
        ];
        this.getUsers();
    }



    getUsers() {
        this._systemRepo.getListSystemUser({})
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = this.utility.prepareNg2SelectData(res, 'id', 'username');
                        console.log(this.users);
                    }
                },
            );
    }



}