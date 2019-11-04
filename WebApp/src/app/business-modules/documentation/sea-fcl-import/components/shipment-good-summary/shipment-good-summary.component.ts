import { Component, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { SeaFCLImportContainerListPopupComponent } from '../popup/container-list/container-list.popup';

import * as fromStore from './../../store/index';
import { ActionsSubject, Store } from '@ngrx/store';
import { Container } from 'src/app/shared/models/document/container.model';

import _uniqBy from 'lodash/uniqBy';
import _groupBy from 'lodash/groupBy';
import { takeUntil } from 'rxjs/operators';


@Component({
    selector: 'shipment-good-summary',
    templateUrl: './shipment-good-summary.component.html',


})
export class SeaFCLImportShipmentGoodSummaryComponent extends AppForm {

    @ViewChild(SeaFCLImportContainerListPopupComponent, { static: false }) containerPopup: SeaFCLImportContainerListPopupComponent;

    description: string = '';
    commodities: string = '';
    containerDetail: string = '';

    grossWeight: number = null;
    netWeight: number = null;
    totalChargeWeight: number = null;
    totalCBM: number = null;

    constructor(
        private _actionStoreSubject: ActionsSubject,
        private _store: Store<fromStore.IContainerState>,
    ) {
        super();
    }

    ngOnInit(): void {
        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromStore.ContainerAction) => {
                    if (action.type === fromStore.ContainerActionTypes.SAVE_CONTAINER) {

                        console.log(action.payload);
                        // * Description, Commondity.
                        this.description = (action.payload || []).reduce((acc: string, curr: Container) => acc += curr.description + "\n", '');
                        this.commodities = (action.payload || []).reduce((acc: string, curr: any) => acc += !!curr.commodityName ? curr.commodityName + ', ' : '', '');

                        console.log(this.commodities);
                        // * GW, Nw, CW, CBM
                        this.grossWeight = (action.payload || []).reduce((acc: string, curr: Container) => acc += curr.gw, 0);
                        this.netWeight = (action.payload || []).reduce((acc: string, curr: Container) => acc += curr.nw, 0);
                        this.totalChargeWeight = (action.payload || []).reduce((acc: string, curr: Container) => acc += curr.chargeAbleWeight, 0);
                        this.totalCBM = (action.payload || []).reduce((acc: string, curr: Container) => acc += curr.cbm, 0);

                        // * Container, Package.
                        this.containerDetail = '';

                        const contObject: any[] = (action.payload || []).map((container: Container | any) => ({
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

                        let packageObject: any[] = (action.payload || []).map((container: Container | any) => ({
                            package: container.packageTypeName,
                            quantity: container.packageQuantity
                        }));

                        // ? If has PackageType & Quantity Package
                        if (!!packageObject.filter(i => Boolean(i)).length) {
                            packageObject = packageObject.filter(i => Boolean(i)); // * Filtering truly and valid value

                            const packageData = [];
                            for (const item of Object.keys(_groupBy(packageObject, 'package'))) {
                                packageData.push({
                                    package: item,
                                    quantity: _groupBy(packageObject, 'package')[item].map(i => i.quantity).reduce((a: any, b: any) => a += b)
                                });
                            }

                            for (const item of packageData) {
                                this.containerDetail += this.handleStringPackage(item);
                            }
                        }
                    }
                }
            );
    }


    handleStringCont(contOb: { cont: string, quantity: number }) {
        return contOb.quantity + 'x' + contOb.cont + ', ';
    }

    handleStringPackage(contOb: { package: string, quantity: number }) {
        return contOb.quantity + 'x' + contOb.package + ', ';
    }

    initContainer() {
        this._store.dispatch(new fromStore.InitContainerAction([]));
    }

    openContainerListPopup() {

        this.containerPopup.show();
    }
}
