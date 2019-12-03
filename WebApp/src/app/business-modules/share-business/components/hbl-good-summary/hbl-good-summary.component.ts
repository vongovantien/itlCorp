import { Component, OnInit } from '@angular/core';
import { ActionsSubject, Store } from '@ngrx/store';

import { ShareBussinessShipmentGoodSummaryComponent } from '../shipment-good-summary/shipment-good-summary.component';
import { Container } from 'src/app/shared/models/document/container.model';

import _groupBy from 'lodash/groupBy';
import * as fromStore from './../../store';


@Component({
    selector: 'hbl-goods-summary',
    templateUrl: './hbl-good-summary.component.html'
})

export class ShareBussinessHBLGoodSummaryComponent extends ShareBussinessShipmentGoodSummaryComponent implements OnInit {

    packageQty: number = null;
    containerDescription: string = '';

    constructor(
        protected _actionStoreSubject: ActionsSubject,
        protected _store: Store<fromStore.IContainerState>,
    ) {
        super(_actionStoreSubject, _store);

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



        // * Container
        this.containerDetail = '';
        this.containerDescription = '';

        containers.forEach((c: Container) => {
            this.containerDescription += this.handleStringContSeal(c.containerNo || '', c.containerTypeName || '', c.sealNo || '');
        });

        const contObject: any[] = (containers || []).map((container: Container | any) => ({
            cont: container.containerTypeName,
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

    handleStringCont(contOb: { cont: string, quantity: number }) {
        return `Part of Container ${contOb.quantity}x${contOb.cont}, `;
    }

    handleStringContSeal(contNo: string = '', contType: string = '', sealNo: string = '') {
        return `${contNo}/${contType}/${sealNo} \n`;
    }
}
