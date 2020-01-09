import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { AirExportDetailHBLComponent } from '../../detail/detail-house-bill.component';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import * as fromShareBussiness from '@share-bussiness';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { AirExportHBLFormCreateComponent } from '../form-create-house-bill-air-export/form-create-house-bill-air-export.component';
import { HouseBill, DIM } from '@models';
import { mergeMap, takeUntil, map, tap, catchError } from 'rxjs/operators';
import { getDetailHBlState, getDimensionVolumesState } from '@share-bussiness';
import { SystemConstants } from 'src/constants/system.const';
import { forkJoin } from 'rxjs';

@Component({
    selector: 'form-separate-house-bill',
    templateUrl: 'form-separate-house-bill.component.html'
})

export class SeparateHouseBillComponent extends AirExportDetailHBLComponent {
    form: FormGroup;
    hblId: string = '';
    jobId: string = '';
    hblDetail: any;


    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<fromShareBussiness.IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _cd: ChangeDetectorRef
    ) {
        super(_progressService,
            _activedRoute,
            _store,
            _documentationRepo,
            _toastService,
            _actionStoreSubject,
            _router,
            _cd);
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.hblId) {
                this.hblId = param.hblId;
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.GetDetailHBLAction(this.hblId));
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                this._store.dispatch(new fromShareBussiness.GetDimensionHBLAction(this.hblId));
            }
        });
    }


    getSeparate() {
        this._documentationRepo.getSeparate(this.hblId)
            .pipe(catchError(this.catchError))
            .subscribe((res: any) => {
                if (!!res) {
                    this.hblId = res.id;
                    console.log(this.hblId);
                    this.hblDetail = res;
                    console.log(this.hblDetail);
                    this.formCreateHBLComponent.updateFormValue(this.hblDetail);

                }
            });
    }

    ngAfterViewInit() {
        this.formCreateHBLComponent.isSeparate = true;
        this._store.select(getDimensionVolumesState)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                map((dims: DIM[]) => dims.map(d => new DIM(d))),
                tap(
                    (dims: DIM[]) => {
                        this.formCreateHBLComponent.dims = dims;
                    }
                ),
                mergeMap(
                    () => this._store.select(getDetailHBlState).pipe(takeUntil(this.ngUnsubscribe))
                )
            )
            .subscribe(
                (hbl: HouseBill) => {
                    if (!!hbl && hbl.id && hbl.id !== SystemConstants.EMPTY_GUID) {
                        this.hblDetail = hbl;
                        console.log(this.hblDetail);
                        this.formCreateHBLComponent.totalCBM = this.hblDetail.cbm;
                        this.formCreateHBLComponent.totalHeightWeight = this.hblDetail.hw;
                        this.formCreateHBLComponent.jobId = this.hblDetail.jobId;
                        this.formCreateHBLComponent.hblId = this.hblDetail.id;
                        this.formCreateHBLComponent.hwconstant = this.hblDetail.hwConstant;
                        this.formCreateHBLComponent.updateFormValue(this.hblDetail);
                        this.getSeparate();
                    }

                }
            );
    }



    saveHBLSeparate() {
        this.formCreateHBLComponent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            this.infoPopup.show();
            return;
        }
        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;
        if (!!this.hblDetail) {
            if (this.hblDetail.parentId == null) {
                houseBill.parentId = this.hblId;
                console.log(houseBill);
                this.createHbl(houseBill);
            } else {
                const modelUpdate = this.getDataForm();
                modelUpdate.id = this.hblId;
                modelUpdate.jobId = this.jobId;
                modelUpdate.parentId = this.hblDetail.parentId;

                for (const dim of modelUpdate.dimensionDetails) {
                    dim.hblId = this.hblId;
                    dim.mblId = this.jobId;
                }
                this.updateHbl(modelUpdate);
            }

        }
    }
}