import { NgModule } from "@angular/core";
import { LAZY_MODULES_MAP, ILazyModules } from "src/app/load-module-map";

export const lazyModulesFClMap: ILazyModules = {
    //AirImport_cdNote: () => import('./detail-job/cd-note/air-import-cd-note.module').then(m => m.AirImportCDNoteModule),
    //AirImport_assignment: () => import('./detail-job/assignment/air-import-assignment.module').then(m => m.AirImportAsignmentModule)
};

@NgModule({
    providers: [
        {
            provide: LAZY_MODULES_MAP,
            useFactory: () => lazyModulesFClMap
        }
    ]
})
export class AirImportLazyLoadModule { }
