import { component$, useSignal } from "@builder.io/qwik";
import {
  UserForm,
  useListLoader,
  deleteUser,
  type User,
} from "../../../components/starter/kool";
import { type DocumentHead } from "@builder.io/qwik-city";
export {
  useListLoader,
  useAddOrUpdate,
} from "../../../components/starter/kool";

export default component$(() => {
  const uesrs = useListLoader();
  const editMode = useSignal(false);
  const user = useSignal<User>({} as any);
 
  return (
    <>
   
      <div class="container container-center">
        <h3>
          <span class="highlight">User List</span>
          <button
            onClick$={() => {
              editMode.value = true;
              user.value = { id: 0, name: "", age: 0 };
            }}
          >
            Add User
          </button>
        </h3>
        {editMode.value ? (
          <UserForm model={user.value} editMode={editMode} />
        ) : (
          uesrs.value.map((it) => (
            <div key={it.id}>
              {it.name}
              {it.age}
              <button
                onClick$={() => {
                  editMode.value = true;
                  user.value = it;
                }}
              >
                Update
              </button>
              <button
                onClick$={async () => {
                  if (confirm("delete sure")) {
                    const data = await deleteUser(it.id);
                    //@ts-ignore
                    uesrs.value=data;
                  }
                }}
              >
                Delete
              </button>
            </div>
          ))
        )}
      </div>
    </>
  );
});

export const head: DocumentHead = {
  title: "Kool",
};
