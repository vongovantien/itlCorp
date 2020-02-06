import { Component, ViewChild } from "@angular/core";
import { AppList } from "src/app/app.list";
import { User } from "@models";
import { SystemRepo } from "@repositories";
import { catchError } from "rxjs/operators";
import { UserLevel } from "src/app/shared/models/system/userlevel";
import { ConfirmPopupComponent } from "@common";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'add-user',
    templateUrl: 'add-user.component.html'
})

export class ShareSystemAddUserComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    constructor(
        private _systemRepo: SystemRepo,
        protected _toastService: ToastrService,

    ) {
        super();
    }
    users: any[] = [];
    headers: CommonInterface.IHeaderTable[] = [];
    positions: CommonInterface.INg2Select[];
    usersLevels: UserLevel[] = [];
    value: any = {};
    userData: User[] = [];
    employeeNames: any[] = [];
    indexRemove: number = null;
    isSubmitted: boolean = false;


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

    public refreshValue(value: any): void {
        this.value = value;
    }

    getUsers() {
        this._systemRepo.getListSystemUser({})
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = res;
                        console.log(this.users);
                    }
                },
            );
    }

    addNewLine() {
        const obj = new UserLevel();
        if (this.usersLevels.length === 0) {
            this.employeeNames = [];
        }
        this.usersLevels.push(obj);
    }

    selectedUser(obj: any, index: number) {
        const objUser = this.users.find(x => x.id === obj.userId);
        this.employeeNames[index] = objUser.employeeNameVn;
    }

    deleteUserLevel(index: number) {
        this.indexRemove = index;
        this.confirmDeletePopup.show();
    }

    onDeleteUserLevel() {
        this.usersLevels.splice(this.indexRemove, 1);
        this.confirmDeletePopup.hide();
    }


    checkValidate() {
        let valid: boolean = true;
        for (const userlv of this.usersLevels) {
            if (

                userlv.userId === null
                || userlv.position === null
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    saveUserLevel() {

        if (!this.usersLevels.length) {
            this._toastService.warning("Please add user Level");
            return;
        }
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }

    }





}