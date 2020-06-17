import { Component } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { NgProgress } from "@ngx-progressbar/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { AppForm } from "src/app/app.form";
import { SettingRepo } from "@repositories";

@Component({
    selector: 'app-unlock-request-add',
    templateUrl: './add-unlock-request.component.html'
})

export class UnlockRequestAddNewComponent extends AppForm {
    //formAdd: FormGroup;
    constructor(
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private _fb: FormBuilder,
        private _settingRepo: SettingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
    }
}