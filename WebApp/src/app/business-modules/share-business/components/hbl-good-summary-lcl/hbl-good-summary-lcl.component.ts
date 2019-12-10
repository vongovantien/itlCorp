import { Component, OnInit, ViewChild } from '@angular/core';
import { ActionsSubject, Store } from '@ngrx/store';

import { ShareBussinessShipmentGoodSummaryComponent } from '../shipment-good-summary/shipment-good-summary.component';
import { Container } from 'src/app/shared/models/document/container.model';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { ShareBussinessGoodsListPopupComponent } from '../goods-list/goods-list.popup';
import { Unit, HouseBill } from 'src/app/shared/models';

import _groupBy from 'lodash/groupBy';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, takeUntil, skip } from 'rxjs/operators';

import * as fromStore from '../../store';
import { getParamsRouterState } from 'src/app/store';
import { Params } from '@angular/router';


@Component({
    selector: 'hbl-goods-summary-lcl',
    templateUrl: './hbl-good-summary-lcl.component.html'
})

export class ShareBussinessHBLGoodSummaryLCLComponent extends ShareBussinessShipmentGoodSummaryComponent implements OnInit {

    @ViewChild(ShareBussinessGoodsListPopupComponent, { static: false }) containerPopup: ShareBussinessGoodsListPopupComponent;

    packageQty: number = null;

    containerDescription: string = '';

    packages: Unit[];
    selectedPackage: any;

    constructor(
        protected _actionStoreSubject: ActionsSubject,
        protected _store: Store<fromStore.IContainerState>,
        private _catalogueRepo: CatalogueRepo
    ) {
        super(_actionStoreSubject, _store);

        this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.PACKAGE })
            .pipe(catchError(this.catchError))
            .subscribe(
                (units: Unit[]) => { this.packages = units || []; }
            );
    }

    ngOnInit(): void {
        this._store.select(getParamsRouterState).subscribe(
            (p: Params) => {
                this.hblid = p['hblId'];
                this.mblid = p['jobId'];
            }
        );

        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromStore.ContainerAction) => {
                    if (action.type === fromStore.ContainerActionTypes.SAVE_CONTAINER) {
                        this.containers = action.payload;
                        this.updateData(action.payload);
                    }
                }
            );

        this.isLocked = this._store.select(fromStore.getTransactionLocked);

        this._store.select(fromStore.getDetailHBlState)
            .pipe(skip(1))
            .subscribe(
                (res: HouseBill) => {
                    if (!!res) {
                        console.log("detail hbl from store", res);
                        this.totalCBM = res.cbm;
                        this.netWeight = res.netWeight;
                        this.totalChargeWeight = res.chargeWeight;
                        this.grossWeight = res.grossWeight;
                        this.containerDetail = res.packageContainer;
                        this.commodities = res.commodity;
                        this.description = res.desOfGoods;
                        this.selectedPackage = [this.packages.find(p => p.id === res.packageType)];
                        this.packageQty = res.packageQty;
                        this.selectedPackage = res.packageType;
                    }
                }
            );
    }

    openContainerListPopup() {
        this.containerPopup.show();
    }

    updateData(containers: Container[] | any) {
        // * Description, Commondity.
        if (!this.description) {
            this.description = (containers || []).filter((c: Container) => Boolean(c.description)).reduce((acc: string, curr: Container) => acc += curr.description + "\n", '');
        }

        const comoditiesName: string[] = containers.map((c: Container) => c.commodityName);

        if (!this.commodities) {
            this.commodities = comoditiesName
                .filter((item: string, index: number) => Boolean(item) && comoditiesName.indexOf(item) === index)
                .reduce((acc: string, curr: any) => acc += curr + "\n", '');
        }

        // * GW, Nw, CW, CBM
        this.grossWeight = (containers || []).reduce((acc: string, curr: Container) => acc += curr.gw, 0);
        this.totalCBM = (containers || []).reduce((acc: string, curr: Container) => acc += curr.cbm, 0);
        this.packageQty = (containers || []).reduce((acc: string, curr: Container) => acc += curr.packageQuantity, 0);

        if (!!containers.length && !this.selectedPackage || containers.length === 1 && !this.selectedPackage) {
            if (!!containers[0].packageTypeId) {
                this.selectedPackage = this.packages.find((unit: Unit) => unit.id === containers[0].packageTypeId).id;
            }
        }
        // * Container
        this.containerDetail = '';
        this.containerDescription = '';

        containers.forEach((c: Container) => {
            this.containerDescription += this.handleStringContSeal(c.containerNo || '', c.containerTypeName || '', c.sealNo || '');
        });

        const contObject: any[] = (containers || []).map((container: Container | any) => ({
            cont: container.containerTypeName || '',
            quantity: container.quantity
        }));

        const contData = [];
        for (const item of Object.keys(_groupBy(contObject, 'cont'))) {
            contData.push({
                cont: item,
                quantity: _groupBy(contObject, 'cont')[item].map(i => i.quantity).reduce((a: any, b: any) => a += b)
            });
        }

        for (const item of contData) {
            this.containerDetail += this.handleStringCont(item);
        }
    }

    initContainer() {
        this._store.dispatch(new fromStore.InitContainerAction([]));
    }

    handleStringCont(contOb: { cont: string, quantity: number }) {
        return `Part of Container ${contOb.quantity}x${!!contOb.cont ? contOb.cont : ''}, `;
    }

    handleStringContSeal(contNo: string = '', contType: string = '', sealNo: string = '') {
        return `${!!contNo ? contNo : '_'}/${!!contType ? contType : '_'}/${!!sealNo ? sealNo : '_'} \n`;
    }
}
