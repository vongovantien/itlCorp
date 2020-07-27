import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { CommercialCreateIncotermComponent } from '../create/create-incoterm-commercial.component';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { Router, ActivatedRoute, ParamMap, Params } from '@angular/router';
import { takeUntil, map, switchMap, tap, catchError } from 'rxjs/operators';

import UUID from 'validator/lib/isUUID';
import { IncotermUpdateModel, Incoterm } from '@models';
import _merge from 'lodash/merge';
import { DataService } from '@services';
@Component({
    selector: 'detail-incoterm-commercial',
    templateUrl: './detail-incoterm-commercial.component.html',
})
export class CommercialDetailIncotermComponent extends CommercialCreateIncotermComponent implements OnInit {

    incotermId: string;

    incotermDetail: IncotermUpdateModel;

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _router: Router,
        private _activedRoute: ActivatedRoute,
        private _cd: ChangeDetectorRef,
        private _dataService: DataService
    ) {
        super(_catalogueRepo, _toastService, _router);
    }

    ngOnInit(): void {
        this._activedRoute.params
            .pipe(
                takeUntil(this.ngUnsubscribe),
                map((p: Params) => {
                    if (p.incotermId && UUID(p.incotermId)) {
                        return p.incotermId;
                    }
                    return null;
                }),
                tap(id => this.incotermId = id),
                switchMap((id) => this._catalogueRepo.getDetailIncoterm(id)),
            ).subscribe(
                (data: IncotermUpdateModel) => {
                    console.log(data);
                    if (!!data) {
                        this.incotermDetail = data;
                        this.updateForm(this.incotermDetail.incoterm);
                        this.setListChargeIncoterm(this.incotermDetail);

                    }
                }
            );
    }

    ngAfterViewInit() {
        this.listChargeSelling.isLoading = true;
        this._cd.detectChanges();
    }

    updateForm(res: Incoterm) {
        this._dataService.setData('incotermService', res.service);

        const formData: Incoterm | any = {
            service: !!res.service ? [{ id: res.service, text: res.service }] : null,
        };
        this.formCreateComponent.formGroup.patchValue(Object.assign(_merge(res, formData)));
    }

    setListChargeIncoterm(data: IncotermUpdateModel) {
        this.listChargeSelling.isLoading = false;
        this.listChargeBuying.isLoading = false;

        this.listChargeSelling.incotermCharges = data.sellings;
        this.listChargeBuying.incotermCharges = data.buyings;
    }

    saveIncoterm(model: IncotermUpdateModel) {
        model.incoterm.id = this.incotermDetail.incoterm.id;
        model.incoterm.userCreated = this.incotermDetail.incoterm.userCreated;

        this._catalogueRepo.updateIncoterm(model).pipe(
            catchError(this.catchError),
        ).subscribe(
            (res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                }
            }
        );
    }
}
