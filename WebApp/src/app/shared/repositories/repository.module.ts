import { NgModule } from '@angular/core';
import { JobRepo, SystemRepo } from './index';
import { CDNoteRepo } from './cdNote.repo';

@NgModule({
    providers: [
        JobRepo,
        SystemRepo,
        CDNoteRepo
    ],
})
export class RepositoryModule {
}