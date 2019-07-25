import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo, CDNoteRepo, AccoutingRepo } from '.';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo,
        AccoutingRepo
    ],
})
export class RepositoryModule {
}