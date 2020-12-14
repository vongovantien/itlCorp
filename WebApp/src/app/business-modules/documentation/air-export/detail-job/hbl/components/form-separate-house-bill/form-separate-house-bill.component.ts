import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import * as fromShareBussiness from '@share-bussiness';
import { CatalogueRepo, DocumentationRepo, ExportRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { HouseBill, CsOtherCharge } from '@models';
import { SystemConstants } from 'src/constants/system.const';

import { AirExportDetailHBLComponent } from '../../detail/detail-house-bill.component';
import { catchError } from 'rxjs/operators';
import { InitShipmentOtherChargeAction } from '@share-bussiness';
import { RoutingConstants } from '@constants';


@Component({
    selector: 'form-separate-house-bill',
    templateUrl: 'form-separate-house-bill.component.html',
    styleUrls: ['./form-separate-house-bill.component.scss']
})

export class SeparateHouseBillComponent extends AirExportDetailHBLComponent implements OnInit {
    form: FormGroup;
    hblId: string = '';
    jobId: string = '';
    hblDetail: any;
    hblSeprateDetail: any;
    hblSeparateId: string;

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _exportRepo: ExportRepo
    ) {
        super(_progressService,
            _activedRoute,
            _store,
            _documentationRepo,
            _catalogueRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _exportRepo);
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this.getSeparate(this.hblId);
            }
        });

    }


    getSeparate(hblId: string) {
        this._documentationRepo.getSeparate(hblId)
            .pipe(catchError(this.catchError))
            .subscribe((res: any) => {
                if (!!res && !!res.id && res.id !== SystemConstants.EMPTY_GUID) {
                    this.hblSeparateId = res.id;
                    this.hblSeprateDetail = res;
                    this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(res.id));
                    this._store.dispatch(new fromShareBussiness.GetDimensionHBLAction(res.id));
                    this._store.dispatch(new fromShareBussiness.GetHBLOtherChargeAction(res.id));
                } else {
                    this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                    this._store.dispatch(new fromShareBussiness.GetDimensionHBLAction(this.hblId));
                    this._store.dispatch(new fromShareBussiness.GetHBLOtherChargeAction(this.hblId));
                }
            });
    }

    ngAfterViewInit() {
        this.formCreateHBLComponent.isSeparate = true;
    }

    onCancel() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl/${this.hblId}`]);
    }

    saveHBLSeparate() {
        this.formCreateHBLComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;
        if (!this.hblSeparateId) {
            houseBill.parentId = this.hblId;
            this.createHbl(houseBill, this.hblId);
        } else {
            const modelUpdate = this.getDataForm();

            modelUpdate.id = this.hblSeparateId;
            modelUpdate.jobId = this.jobId;
            modelUpdate.parentId = this.hblId;

            for (const dim of modelUpdate.dimensionDetails) {
                dim.hblid = this.hblSeparateId;
            }
            this.updateHbl(modelUpdate, true);
        }
    }
}