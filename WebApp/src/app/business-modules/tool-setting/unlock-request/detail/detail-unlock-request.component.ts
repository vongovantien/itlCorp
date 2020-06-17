import { Component } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { Router } from "@angular/router";
import { FormBuilder } from "@angular/forms";
import { NgProgress } from "@ngx-progressbar/core";
import { AppForm } from "src/app/app.form";
import { SettingRepo } from "@repositories";

@Component({
    selector: 'app-unlock-request-detail',
    templateUrl: './detail-unlock-request.component.html'
})

export class UnlockRequestDetailComponent extends AppForm {
    constructor(
        private _toastService: ToastrService,
        private _router: Router,
        private _fb: FormBuilder,
        private _progressService: NgProgress,
        private _settingRepo: SettingRepo,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() { }
}