import { component$ } from "@builder.io/qwik";

import styles from "./footer.module.css";
import { useServerTimeLoader } from "~/routes/layout";

export default component$(() => {
  const serverTime = useServerTimeLoader();

  return (
    <footer>
      <div class="container">
        <a href="https://www.builder.io/" target="_blank" class={styles.anchor}>
          <span>Made with ♡ by Builder.io</span>
          <span class={styles.spacer}>|</span>
           <span>{serverTime.value.date}</span> 
        </a>
      </div>
    </footer>
  );
});
