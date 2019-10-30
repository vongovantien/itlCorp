import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DocumentationRepo, CatalogueRepo } from 'src/app/shared/repositories';
import { forkJoin } from 'rxjs';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'form-create-sea-fcl-import',
    templateUrl: './form-create-sea-fcl-import.component.html',
})
export class SeaFClImportFormCreateComponent extends AppForm {

    ladingTypes: CommonInterface.IValueDisplay[];
    shipmentTypes: CommonInterface.IValueDisplay[];
    serviceTypes: CommonInterface.IValueDisplay[];

    configComboGridPartner: CommonInterface.IComboGirdConfig;
    selectedCarrier: Partial<CommonInterface.IComboGridData | any> = {};
    selectedAgent: Partial<CommonInterface.IComboGridData | any> = {};

    carries: any[] = [];
    agents: any[] = [];

    constructor(
        private _documentRepo: DocumentationRepo,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
    }


    ngOnInit(): void {
        this.configComboGridPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'PartnerID' },
                { field: 'shortName', label: 'Name Abbr' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ]
        }, { selectedDisplayFields: ['shortName'], });
        this.getServiceType();
        this.getPartner();
    }

    getServiceType() {
        this._documentRepo.getShipmentDataCommon()
            .subscribe(
                (res: ICommonShipmentData) => {
                    this.serviceTypes = res.serviceTypes;
                    this.ladingTypes = res.billOfLadings;
                    this.shipmentTypes = res.shipmentTypes;
                }
            );
    }

    getPartner() {
        forkJoin([
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER),
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.AGENT),
        ]).pipe(catchError(this.catchError))
            .subscribe(
                ([carries, agents]: any[] = [[], []]) => {
                    this.carries = carries;
                    this.agents = agents;
                }
            );
    }

    onSelectDataFormInfo(data: any, key: string | any) {
        switch (key) {
            case 'customer':
                this.selectedCarrier = { field: 'shortName', value: data.shortName, data: data };
                break;
            default:
                break;
        }
    }
}

interface ICommonShipmentData {
    billOfLadings: CommonInterface.IValueDisplay[];
    freightTerms: CommonInterface.IValueDisplay[];
    serviceTypes: CommonInterface.IValueDisplay[];
    shipmentTypes: CommonInterface.IValueDisplay[];
    typeOfMoves: CommonInterface.IValueDisplay[];
}
