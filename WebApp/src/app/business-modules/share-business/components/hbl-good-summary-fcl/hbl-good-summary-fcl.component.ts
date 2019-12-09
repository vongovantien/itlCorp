import { Component, OnInit } from '@angular/core';
import { ActionsSubject, Store } from '@ngrx/store';

import { ShareBussinessShipmentGoodSummaryComponent } from '../shipment-good-summary/shipment-good-summary.component';
import { Container } from 'src/app/shared/models/document/container.model';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { Unit, HouseBill } from 'src/app/shared/models';

import _groupBy from 'lodash/groupBy';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, skip } from 'rxjs/operators';

import * as fromStore from '../../store';


@Component({
    selector: 'hbl-goods-summary-fcl',
    templateUrl: './hbl-good-summary-fcl.component.html'
})

export class ShareBussinessHBLGoodSummaryFCLComponent extends ShareBussinessShipmentGoodSummaryComponent implements OnInit {

    packageQty: number = null;

    containerDescription: string = '';

    packages: Unit[];
    selectedPackage: number;

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

        this._store.select(fromStore.getDetailHBlState)
            .pipe(skip(1))
            .subscribe(
                (res: HouseBill) => {
                    if (!!res) {
                        this.totalCBM = res.cbm;
                        this.netWeight = res.netWeight;
                        this.totalChargeWeight = res.chargeWeight;
                        this.grossWeight = res.grossWeight;
                        this.containerDetail = res.packageContainer;
                        this.commodities = res.commodity;
                        this.description = res.desOfGoods;
                        this.selectedPackage = res.packageType;
                    }
                }
            );
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
        this.netWeight = (containers || []).reduce((acc: string, curr: Container) => acc += curr.nw, 0);
        this.totalChargeWeight = (containers || []).reduce((acc: string, curr: Container) => acc += curr.chargeAbleWeight, 0);
        this.totalCBM = (containers || []).reduce((acc: string, curr: Container) => acc += curr.cbm, 0);
        this.packageQty = (containers || []).reduce((acc: string, curr: Container) => acc += curr.packageQuantity, 0);

        if (!!containers.length && !this.selectedPackage || containers.length === 1) {
            this.selectedPackage = this.packages.find((unit: Unit) => unit.id === containers[0].packageTypeId).id;
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
        this.containerDetail = this.containerDetail.trim().replace(/\,$/, "");

        // let packageObject: any[] = (containers || []).map((container: Container | any) => {
        //     if (container.packageTypeName && container.packageQuantity) {
        //         return {
        //             package: container.packageTypeName,
        //             quantity: container.packageQuantity
        //         };
        //     }
        // });

        // // ? If has PackageType & Quantity Package
        // if (!!packageObject.filter(i => Boolean(i)).length) {
        //     packageObject = packageObject.filter(i => Boolean(i)); // * Filtering truly and valid value

        //     const packageData = [];
        //     for (const item of Object.keys(_groupBy(packageObject, 'package'))) {
        //         packageData.push({
        //             package: item,
        //             quantity: _groupBy(packageObject, 'package')[item].map(i => i.quantity).reduce((a: any, b: any) => a += b)
        //         });
        //     }

        //     for (const item of packageData) {
        //         this.containerDetail += this.handleStringPackage(item);
        //     }
        // }
    }

    initContainer() {
        this._store.dispatch(new fromStore.InitContainerAction([]));
    }

    handleStringContSeal(contNo: string = '', contType: string = '', sealNo: string = '') {
        return `${!!contNo ? contNo : '_'}/${!!contType ? contType : '_'}/${!!sealNo ? sealNo : '_'} \n`;
    }
}
