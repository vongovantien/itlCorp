import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { UserGroup } from 'src/app/shared/models/system/userGroup.model';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'form-user-group',
    templateUrl: './form-user-group.component.html'
})
export class FormUserGroupComponent extends PopupBase implements OnInit {
    @Input() title: string;
    @Input() userGroup: UserGroup = null;
    formGroup: FormGroup;
    isSubmitted: boolean = false;

    users: any[] = [];
    levelPemissions: any[] = [];
    positions: any[] = [];
    permissions: any[] = [];

    user: AbstractControl;

    constructor(private _fb: FormBuilder,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formGroup = this._fb.group({
            user: ['', Validators.compose([
                Validators.required,
            ])],
            level: [''],
            position: [''],
            permission: ['']
        });
        this.user = this.formGroup.controls['user'];
    }
    setValueFormGroup(res: any) {
        if (this.users.length > 0) {
            this.formGroup.setValue({
                user: this.users.filter(i => i.id === res.userId)[0] || null,
                level: '',
                position: '',
                permission: ''
            });
        }
    }
    addUserToGroup() {
        this.isSubmitted = true;
        if (this.formGroup.valid) {
            this._progressRef.start();
            const body: any = {
                groupId: this.userGroup.groupId,
                userId: this.user.value
            };
            this._systemRepo.addUserToGroup(body)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                        this.isSubmitted = false;
                        this.initForm();
                    })
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
}
