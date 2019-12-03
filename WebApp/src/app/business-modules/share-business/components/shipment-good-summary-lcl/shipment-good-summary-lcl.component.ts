import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Unit } from 'src/app/shared/models';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'shipment-good-summary-lcl',
    templateUrl: './shipment-good-summary-lcl.component.html'
})

export class ShareBussinessShipmentGoodSummaryLCLComponent extends AppForm implements OnInit {

    packages: CommonInterface.INg2Select[];
    commodities: string = null;
    gw: number = null;
    cbm: number = null;
    packageQuantity: number = null;
    packageTypes: CommonInterface.INg2Select[] = [];

    constructor(
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }

    ngOnInit() {
        this.getPackage();
    }

    getPackage() {
        this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.PACKAGE })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.packages = this.utility.prepareNg2SelectData(res, 'code', 'unitNameEn');
                }
            );
    }

    selected($event: any) {
        if (!this.packageTypes.length) {
            this.packageTypes.push($event);
        }
        console.log(this.packageTypes);
    }

    removed($event: any) {
        console.log(this.packageTypes);
    }
}
