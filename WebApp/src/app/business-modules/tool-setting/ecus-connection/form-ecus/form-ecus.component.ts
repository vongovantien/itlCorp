import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemRepo, OperationRepo } from '@repositories';
import { FormBuilder, FormGroup, AbstractControl } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

import { EcusConnection } from 'src/app/shared/models/tool-setting/ecus-connection';

import merge from 'lodash/merge';

@Component({
    selector: 'form-ecus-popup',
    templateUrl: './form-ecus.component.html',
})
export class EcusConnectionFormPopupComponent extends PopupBase implements OnInit {
    @Output() onUpdate: EventEmitter<boolean> = new EventEmitter<boolean>();

    formGroup: FormGroup;
    userId: AbstractControl;

    Users: CommonInterface.INg2Select[] = [];

    datetimeCreated: string;
    userCreatedName: string;
    datetimeModified: string;
    userModifiedName: string;

    userCreated: string;

    title: string = 'Add new Ecus';

    isAllowUpdate: boolean = true;

    constructor(
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _toast: ToastrService,
        private _operationRepo: OperationRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.getListUsers();
        this.initForm();
    }

    initForm() {
        this.formGroup = this._fb.group({
            id: [0],
            name: [],
            userId: [],
            serverName: [],
            dbname: [],
            dbusername: [],
            dbpassword: [],
            note: [],
            active: [true],

        });

        this.userId = this.formGroup.controls['userId'];
    }

    getListUsers() {
        this._systemRepo.getSystemUsers().subscribe((data: any) => {
            this.Users = this.utility.prepareNg2SelectData(data, 'id', 'username');
        });
    }

    onSaveEcus() {
        const valueForm = this.formGroup.getRawValue();
        const ecusUserId = !!valueForm.userId && !!valueForm.userId.length ? valueForm.userId[0].id : null;

        const ecus: EcusConnection = new EcusConnection(merge(valueForm, Object.assign({}, { userId: ecusUserId })));

        if (!this.isShowUpdate) {
            ecus.id = 0;
            this._operationRepo.addNewEcus(ecus)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toast.success(res.message);
                            this.onUpdate.emit(true);
                            this.hide();
                            return;
                        }
                        this._toast.error(res.message);
                    });
        } else {
            ecus.userCreated = this.userCreated;
            ecus.datetimeCreated = this.datetimeCreated;

            this._operationRepo.updateEcus(ecus).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success(res.message);
                        this.onUpdate.emit(true);
                        this.hide();
                        return;
                    }
                    this._toast.error(res.message);
                });
        }

    }

}
