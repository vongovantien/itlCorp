import { Injectable } from "@angular/core";
import { Actions, createEffect, ofType } from "@ngrx/effects";
import { DocumentationRepo } from "@repositories";
import { LoadListWorkOrderSuccess, LoadListWorkOrderFail, WorkOrderActionTypes, LoadDetailWorkOrderSuccess, LoadDetailWorkOrderFail, DeletePriceItemWorkOrderSuccess, DeletePriceItemWorkOrderFail } from "../actions";
import { map, catchError, switchMap } from "rxjs/operators";
import { Observable, of } from "rxjs";
import { Action } from "@ngrx/store";
import { ToastrService } from "ngx-toastr";

@Injectable()
export class WorkOrderEffects {

    constructor(
        private actions$: Actions,
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService
    ) { }

    getListWorkOrderEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(WorkOrderActionTypes.LOAD_LIST),
            switchMap(
                (param: CommonInterface.IParamPaging) => this._documentationRepo.getListWorkOrder(param.page, param.size, param.dataSearch)
                    .pipe(
                        catchError(() => of(LoadListWorkOrderFail())),
                        map((data: CommonInterface.IResponsePaging) => LoadListWorkOrderSuccess(data)),
                    )
            )
        ));

    // getDetailWorkOrderEffect$: Observable<Action> = createEffect(() => this.actions$
    //     .pipe(
    //         ofType(WorkOrderActionTypes.LOAD_DETAIL),
    //         switchMap(
    //             (param: { id: string }) => this._documentationRepo.getDetailWorkOrder(param.id)
    //                 .pipe(
    //                     catchError(() => of(LoadDetailWorkOrderFail())),
    //                     map((data: WorkOrderViewUpdateModel) => LoadDetailWorkOrderSuccess(data)),
    //                 )
    //         )
    //     ));

    deletePriceItemWorkOrderEffect$: Observable<Action> = createEffect(() => this.actions$
        .pipe(
            ofType(WorkOrderActionTypes.DELETE_PRICE_ITEM),
            switchMap(
                (param: { index: number, id: string }) => this._documentationRepo.deletePriceItem(param.id)
                    .pipe(
                        catchError(() => of(DeletePriceItemWorkOrderFail())),
                        map((data: CommonInterface.IResult) => {
                            if (data.status) {
                                this._toastService.success(data.message);
                                return DeletePriceItemWorkOrderSuccess({ index: param.index })
                            }
                            this._toastService.error(data.message);
                            return DeletePriceItemWorkOrderFail();
                        }),
                    )
            )
        ));
}


